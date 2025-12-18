const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

// Constantes para armazenamento local
const TOKEN_KEY = 'auth_token';
const USER_KEY = 'auth_user';

export interface ApiResponse<T> {
    success: boolean;
    message: string;
    data: T;
    errors: string[] | null;
}

export interface ManagerOption {
    id: string;
    name: string;
    roleName: string;
}

export interface PhoneRequest {
    number: string;
    type: string;
}

export interface SelfRegisterRequest {
    name: string;
    email: string;
    documentNumber: string;
    password: string;
    confirmPassword: string;
    birthDate: string;
    roleId: string;
    managerId: string;
    phones: PhoneRequest[];
}

export interface RoleOption {
    id: string;
    name: string;
    hierarchyLevel: number;
}

export interface LoginRequest {
    email: string;
    password: string;
}

export interface RoleInfo {
    id: string;
    name: string;
    hierarchyLevel: number;
}

export interface UserInfo {
    id: string;
    name: string;
    email: string;
    role: RoleInfo;
    canApproveRegistrations: boolean;
    canCreateEmployees: boolean;
    canEditEmployees: boolean;
    canDeleteEmployees: boolean;
    canManageRoles: boolean;
    pendingApprovals: number;
}

export interface TokenResponse {
    accessToken: string;
    tokenType: string;
    expiresIn: number;
    user: UserInfo;
}

// ===== AUTENTICAÇÃO =====

// Login
export async function login(data: LoginRequest): Promise<TokenResponse> {
    const response = await fetch(`${API_BASE_URL}/Auth/login`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
    });
    
    const result: ApiResponse<TokenResponse> = await response.json();
    
    if (!result.success) {
        const errorMessage = result.errors?.join(', ') || result.message || 'E-mail ou senha inválidos';
        throw new Error(errorMessage);
    }
    
    // Salvar token e usuário no localStorage
    localStorage.setItem(TOKEN_KEY, result.data.accessToken);
    localStorage.setItem(USER_KEY, JSON.stringify(result.data.user));
    
    return result.data;
}

// Logout
export function logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
}

// Verificar se está autenticado
export function isAuthenticated(): boolean {
    return !!localStorage.getItem(TOKEN_KEY);
}

// Obter token
export function getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
}

// Obter usuário logado
export function getCurrentUser(): UserInfo | null {
    const user = localStorage.getItem(USER_KEY);
    return user ? JSON.parse(user) : null;
}

// Verificar se já existe um diretor
export async function hasDirector(): Promise<boolean> {
    const response = await fetch(`${API_BASE_URL}/Auth/has-director`);
    const result: ApiResponse<boolean> = await response.json();
    return result.data;
}

// Buscar gerentes disponíveis (público)
export async function getAvailableManagers(): Promise<ManagerOption[]> {
    const response = await fetch(`${API_BASE_URL}/Employees/managers`);
    const result: ApiResponse<ManagerOption[]> = await response.json();
    
    if (!result.success) {
        throw new Error(result.message || 'Erro ao buscar gerentes');
    }
    
    return result.data;
}

// Buscar cargos disponíveis para registro (público) - Excluindo Diretor
export async function getAvailableRoles(): Promise<RoleOption[]> {
    const response = await fetch(`${API_BASE_URL}/Roles/public`);
    const result: ApiResponse<RoleOption[]> = await response.json();
    
    if (!result.success) {
        throw new Error(result.message || 'Erro ao buscar cargos');
    }
    
    return result.data;
}

// Auto-registro de funcionário
export async function selfRegister(data: SelfRegisterRequest): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/Auth/self-register`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
    });
    
    const result: ApiResponse<any> = await response.json();
    
    if (!result.success) {
        const errorMessage = result.errors?.join(', ') || result.message || 'Erro ao realizar cadastro';
        throw new Error(errorMessage);
    }
}

// ===== FUNCIONÁRIOS =====

export interface PhoneDto {
    id?: string;
    number: string;
    type: string;
}

export interface EmployeeDto {
    id: string;
    name: string;
    email: string;
    documentNumber: string;
    birthDate: string;
    age: number;
    role: RoleInfo;
    managerId: string | null;
    managerName: string | null;
    enabled: boolean;
    approvedAt: string | null;
    approvedByName: string | null;
    phones: PhoneDto[];
    photoUrl: string | null;
    createdAt: string;
    updatedAt: string | null;
}

export interface UpdateEmployeeRequest {
    name: string;
    email: string;
    documentNumber: string;
    birthDate: string;
    roleId: string;
    managerId: string | null;
    phones: PhoneDto[];
}

// Header de autenticação
function getAuthHeaders(): HeadersInit {
    const token = getToken();
    return {
        'Content-Type': 'application/json',
        ...(token ? { 'Authorization': `Bearer ${token}` } : {}),
    };
}

// Listar todos os funcionários
export async function getEmployees(): Promise<EmployeeDto[]> {
    const response = await fetch(`${API_BASE_URL}/Employees`, {
        headers: getAuthHeaders(),
    });
    
    if (response.status === 401) {
        logout();
        throw new Error('Sessão expirada. Faça login novamente.');
    }
    
    const result: ApiResponse<EmployeeDto[]> = await response.json();
    
    if (!result.success) {
        throw new Error(result.message || 'Erro ao buscar funcionários');
    }
    
    return result.data;
}

// Buscar funcionário por ID
export async function getEmployeeById(id: string): Promise<EmployeeDto> {
    const response = await fetch(`${API_BASE_URL}/Employees/${id}`, {
        headers: getAuthHeaders(),
    });
    
    if (response.status === 401) {
        logout();
        throw new Error('Sessão expirada. Faça login novamente.');
    }
    
    if (response.status === 404) {
        throw new Error('Funcionário não encontrado');
    }
    
    const result: ApiResponse<EmployeeDto> = await response.json();
    
    if (!result.success) {
        throw new Error(result.message || 'Erro ao buscar funcionário');
    }
    
    return result.data;
}

export interface CreateEmployeeRequest {
    name: string;
    email: string;
    documentNumber: string;
    password: string;
    birthDate: string;
    roleId: string;
    managerId?: string;
    phones: PhoneRequest[];
}

// Criar funcionário
export async function createEmployee(data: CreateEmployeeRequest): Promise<EmployeeDto> {
    const response = await fetch(`${API_BASE_URL}/Employees`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(data),
    });
    
    if (response.status === 401) {
        logout();
        throw new Error('Sessão expirada. Faça login novamente.');
    }
    
    const result: ApiResponse<EmployeeDto> = await response.json();
    
    if (!result.success) {
        const errorMessage = result.errors?.join(', ') || result.message || 'Erro ao criar funcionário';
        throw new Error(errorMessage);
    }
    
    return result.data;
}

// Atualizar funcionário
export async function updateEmployee(id: string, data: UpdateEmployeeRequest): Promise<EmployeeDto> {
    const response = await fetch(`${API_BASE_URL}/Employees/${id}`, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: JSON.stringify(data),
    });
    
    if (response.status === 401) {
        logout();
        throw new Error('Sessão expirada. Faça login novamente.');
    }
    
    const result: ApiResponse<EmployeeDto> = await response.json();
    
    if (!result.success) {
        const errorMessage = result.errors?.join(', ') || result.message || 'Erro ao atualizar funcionário';
        throw new Error(errorMessage);
    }
    
    return result.data;
}

// Excluir funcionário
export async function deleteEmployee(id: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/Employees/${id}`, {
        method: 'DELETE',
        headers: getAuthHeaders(),
    });
    
    if (response.status === 401) {
        logout();
        throw new Error('Sessão expirada. Faça login novamente.');
    }
    
    const result: ApiResponse<any> = await response.json();
    
    if (!result.success) {
        throw new Error(result.message || 'Erro ao excluir funcionário');
    }
}

// Buscar cargos atribuíveis (autenticado)
export async function getAssignableRoles(): Promise<RoleOption[]> {
    const response = await fetch(`${API_BASE_URL}/Roles/assignable`, {
        headers: getAuthHeaders(),
    });
    
    if (response.status === 401) {
        logout();
        throw new Error('Sessão expirada. Faça login novamente.');
    }
    
    const result: ApiResponse<RoleOption[]> = await response.json();
    
    if (!result.success) {
        throw new Error(result.message || 'Erro ao buscar cargos');
    }
    
    return result.data;
}

// Buscar gerentes (autenticado)
export async function getManagersForEdit(): Promise<ManagerOption[]> {
    const response = await fetch(`${API_BASE_URL}/Employees/managers`, {
        headers: getAuthHeaders(),
    });
    
    const result: ApiResponse<ManagerOption[]> = await response.json();
    
    if (!result.success) {
        throw new Error(result.message || 'Erro ao buscar gerentes');
    }
    
    return result.data;
}

// ===== CARGOS (ROLES) =====

export interface RoleDto {
    id: string;
    name: string;
    description: string;
    hierarchyLevel: number;
    canApproveRegistrations: boolean;
    canCreateEmployees: boolean;
    canEditEmployees: boolean;
    canDeleteEmployees: boolean;
    canManageRoles: boolean;
    employeeCount: number;
    createdAt: string;
    updatedAt: string | null;
}

export interface CreateRoleRequest {
    name: string;
    description: string;
    hierarchyLevel: number;
    canApproveRegistrations: boolean;
    canCreateEmployees: boolean;
    canEditEmployees: boolean;
    canDeleteEmployees: boolean;
    canManageRoles: boolean;
}

export interface UpdateRoleRequest {
    name: string;
    description: string;
    hierarchyLevel: number;
    canApproveRegistrations: boolean;
    canCreateEmployees: boolean;
    canEditEmployees: boolean;
    canDeleteEmployees: boolean;
    canManageRoles: boolean;
}

// Listar todos os cargos
export async function getRoles(): Promise<RoleDto[]> {
    const response = await fetch(`${API_BASE_URL}/Roles`, {
        headers: getAuthHeaders(),
    });
    
    if (response.status === 401) {
        logout();
        throw new Error('Sessão expirada. Faça login novamente.');
    }
    
    const result: ApiResponse<RoleDto[]> = await response.json();
    
    if (!result.success) {
        throw new Error(result.message || 'Erro ao buscar cargos');
    }
    
    return result.data;
}

// Buscar cargo por ID
export async function getRoleById(id: string): Promise<RoleDto> {
    const response = await fetch(`${API_BASE_URL}/Roles/${id}`, {
        headers: getAuthHeaders(),
    });
    
    if (response.status === 401) {
        logout();
        throw new Error('Sessão expirada. Faça login novamente.');
    }
    
    if (response.status === 404) {
        throw new Error('Cargo não encontrado');
    }
    
    const result: ApiResponse<RoleDto> = await response.json();
    
    if (!result.success) {
        throw new Error(result.message || 'Erro ao buscar cargo');
    }
    
    return result.data;
}

// Criar cargo
export async function createRole(data: CreateRoleRequest): Promise<RoleDto> {
    const response = await fetch(`${API_BASE_URL}/Roles`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(data),
    });
    
    if (response.status === 401) {
        logout();
        throw new Error('Sessão expirada. Faça login novamente.');
    }
    
    const result: ApiResponse<RoleDto> = await response.json();
    
    if (!result.success) {
        const errorMessage = result.errors?.join(', ') || result.message || 'Erro ao criar cargo';
        throw new Error(errorMessage);
    }
    
    return result.data;
}

// Atualizar cargo
export async function updateRole(id: string, data: UpdateRoleRequest): Promise<RoleDto> {
    const response = await fetch(`${API_BASE_URL}/Roles/${id}`, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: JSON.stringify(data),
    });
    
    if (response.status === 401) {
        logout();
        throw new Error('Sessão expirada. Faça login novamente.');
    }
    
    const result: ApiResponse<RoleDto> = await response.json();
    
    if (!result.success) {
        const errorMessage = result.errors?.join(', ') || result.message || 'Erro ao atualizar cargo';
        throw new Error(errorMessage);
    }
    
    return result.data;
}

// Excluir cargo
export async function deleteRole(id: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/Roles/${id}`, {
        method: 'DELETE',
        headers: getAuthHeaders(),
    });
    
    if (response.status === 401) {
        logout();
        throw new Error('Sessão expirada. Faça login novamente.');
    }
    
    const result: ApiResponse<any> = await response.json();
    
    if (!result.success) {
        throw new Error(result.message || 'Erro ao excluir cargo');
    }
}

// ===== PERFIL =====

export interface UpdateProfileRequest {
    name: string;
    email: string;
    phones: PhoneRequest[];
}

export interface ChangePasswordRequest {
    currentPassword: string;
    newPassword: string;
    confirmPassword: string;
}

// Atualizar próprio perfil
export async function updateProfile(data: UpdateProfileRequest): Promise<EmployeeDto> {
    const response = await fetch(`${API_BASE_URL}/Employees/profile`, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: JSON.stringify(data),
    });
    
    if (response.status === 401) {
        logout();
        throw new Error('Sessão expirada. Faça login novamente.');
    }
    
    const result: ApiResponse<EmployeeDto> = await response.json();
    
    if (!result.success) {
        const errorMessage = result.errors?.join(', ') || result.message || 'Erro ao atualizar perfil';
        throw new Error(errorMessage);
    }
    
    return result.data;
}

// Alterar própria senha
export async function changePassword(data: ChangePasswordRequest): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/Employees/change-password`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(data),
    });
    
    if (response.status === 401) {
        logout();
        throw new Error('Sessão expirada. Faça login novamente.');
    }
    
    const result: ApiResponse<any> = await response.json();
    
    if (!result.success) {
        const errorMessage = result.errors?.join(', ') || result.message || 'Erro ao alterar senha';
        throw new Error(errorMessage);
    }
}

// Resetar senha de um funcionário (usado por gestores)
export async function resetPassword(employeeId: string): Promise<string> {
    const response = await fetch(`${API_BASE_URL}/Employees/${employeeId}/reset-password`, {
        method: 'POST',
        headers: getAuthHeaders(),
    });
    
    if (response.status === 401) {
        logout();
        throw new Error('Sessão expirada. Faça login novamente.');
    }
    
    const result: ApiResponse<{ temporaryPassword: string }> = await response.json();
    
    if (!result.success) {
        const errorMessage = result.errors?.join(', ') || result.message || 'Erro ao resetar senha';
        throw new Error(errorMessage);
    }
    
    return result.data.temporaryPassword;
}

// Upload de foto de perfil
export async function uploadPhoto(employeeId: string, file: File): Promise<string> {
    const formData = new FormData();
    formData.append('file', file);
    
    const token = getToken();
    const response = await fetch(`${API_BASE_URL}/Employees/${employeeId}/photo`, {
        method: 'POST',
        headers: {
            ...(token && { 'Authorization': `Bearer ${token}` }),
        },
        body: formData,
    });
    
    if (response.status === 401) {
        logout();
        throw new Error('Sessão expirada. Faça login novamente.');
    }
    
    const result: ApiResponse<{ photoUrl: string }> = await response.json();
    
    if (!result.success) {
        const errorMessage = result.errors?.join(', ') || result.message || 'Erro ao fazer upload da foto';
        throw new Error(errorMessage);
    }
    
    return result.data.photoUrl;
}

// Remover foto de perfil
export async function deletePhoto(employeeId: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/Employees/${employeeId}/photo`, {
        method: 'DELETE',
        headers: getAuthHeaders(),
    });
    
    if (response.status === 401) {
        logout();
        throw new Error('Sessão expirada. Faça login novamente.');
    }
    
    const result: ApiResponse<any> = await response.json();
    
    if (!result.success) {
        throw new Error(result.message || 'Erro ao remover foto');
    }
}
