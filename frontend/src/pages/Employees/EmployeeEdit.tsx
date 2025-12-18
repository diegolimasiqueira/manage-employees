import { useEffect, useState, useMemo } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useDispatch } from 'react-redux';
import { setPageTitle } from '../../store/themeConfigSlice';
import {
    getEmployeeById,
    updateEmployee,
    getAssignableRoles,
    getManagersForEdit,
    getCurrentUser,
    resetPassword,
    uploadPhoto,
    deletePhoto,
    EmployeeDto,
    RoleOption,
    ManagerOption,
    PhoneDto,
    UpdateEmployeeRequest,
} from '../../services/api';
import Swal from 'sweetalert2';
import Breadcrumb from '../../components/Breadcrumb';
import IconPlus from '../../components/Icon/IconPlus';
import IconTrash from '../../components/Icon/IconTrash';
import IconLock from '../../components/Icon/IconLockDots';

interface PhoneField {
    id: string;
    number: string;
    type: string;
}

interface FormErrors {
    name?: string;
    email?: string;
    documentNumber?: string;
    birthDate?: string;
    roleId?: string;
    managerId?: string;
    phones?: string;
}

const EmployeeEdit = () => {
    const dispatch = useDispatch();
    const navigate = useNavigate();
    const { id } = useParams<{ id: string }>();
    
    // Memorizar currentUser para evitar recriação a cada render
    const currentUser = useMemo(() => getCurrentUser(), []);

    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [employee, setEmployee] = useState<EmployeeDto | null>(null);
    const [roles, setRoles] = useState<RoleOption[]>([]);
    const [managers, setManagers] = useState<ManagerOption[]>([]);
    const [errors, setErrors] = useState<FormErrors>({});

    // Form fields
    const [name, setName] = useState('');
    const [email, setEmail] = useState('');
    const [documentNumber, setDocumentNumber] = useState('');
    const [birthDate, setBirthDate] = useState('');
    const [roleId, setRoleId] = useState('');
    const [managerId, setManagerId] = useState('');
    const [phones, setPhones] = useState<PhoneField[]>([]);
    const [photoFile, setPhotoFile] = useState<File | null>(null);
    const [photoPreview, setPhotoPreview] = useState<string | null>(null);
    const [currentPhotoUrl, setCurrentPhotoUrl] = useState<string | null>(null);

    useEffect(() => {
        dispatch(setPageTitle('Editar Funcionário'));
        
        if (!id) {
            navigate('/employees');
            return;
        }

        // Verificar permissão
        if (!currentUser?.canEditEmployees) {
            Swal.fire({
                icon: 'warning',
                title: 'Sem Permissão',
                text: 'Você não tem permissão para editar funcionários.',
                confirmButtonColor: '#006B3F',
            }).then(() => navigate('/employees'));
            return;
        }

        // Verificar se está tentando editar a si mesmo
        if (id === currentUser?.id) {
            Swal.fire({
                icon: 'warning',
                title: 'Ação não permitida',
                text: 'Você não pode editar seu próprio cadastro. Solicite a um gestor.',
                confirmButtonColor: '#006B3F',
            }).then(() => navigate('/employees'));
            return;
        }

        loadData();
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [id]);

    const loadData = async () => {
        try {
            setLoading(true);
            
            // Carregar dados em paralelo
            const [employeeData, rolesData, managersData] = await Promise.all([
                getEmployeeById(id!),
                getAssignableRoles(),
                getManagersForEdit(),
            ]);

            setEmployee(employeeData);
            setRoles(rolesData);
            setManagers(managersData.filter(m => m.id !== id)); // Excluir o próprio funcionário

            // Preencher formulário
            setName(employeeData.name);
            setEmail(employeeData.email);
            setDocumentNumber(formatCPF(employeeData.documentNumber));
            setBirthDate(employeeData.birthDate.split('T')[0]);
            setRoleId(employeeData.role.id);
            setManagerId(employeeData.managerId || '');
            setPhones(
                employeeData.phones.map((p, idx) => ({
                    id: p.id || `phone-${idx}`,
                    number: formatPhone(p.number),
                    type: p.type,
                }))
            );
            
            // Carregar foto atual
            if (employeeData.photoUrl) {
                setCurrentPhotoUrl(`http://localhost:5000${employeeData.photoUrl}`);
            }
        } catch (error: any) {
            Swal.fire({
                icon: 'error',
                title: 'Erro',
                text: error.message || 'Erro ao carregar dados do funcionário',
                confirmButtonColor: '#006B3F',
            }).then(() => navigate('/employees'));
        } finally {
            setLoading(false);
        }
    };

    const formatCPF = (value: string) => {
        const numbers = value.replace(/\D/g, '');
        return numbers.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
    };

    const formatPhone = (value: string) => {
        const numbers = value.replace(/\D/g, '');
        if (numbers.length <= 10) {
            return numbers.replace(/(\d{2})(\d{4})(\d{4})/, '($1) $2-$3');
        }
        return numbers.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
    };

    const handleCPFChange = (value: string) => {
        const numbers = value.replace(/\D/g, '').slice(0, 11);
        setDocumentNumber(formatCPF(numbers));
    };

    const handlePhoneChange = (index: number, value: string) => {
        const numbers = value.replace(/\D/g, '').slice(0, 11);
        const formatted = formatPhone(numbers);
        const newPhones = [...phones];
        newPhones[index].number = formatted;
        setPhones(newPhones);
    };

    const handlePhoneTypeChange = (index: number, type: string) => {
        const newPhones = [...phones];
        newPhones[index].type = type;
        setPhones(newPhones);
    };

    const addPhone = () => {
        setPhones([...phones, { id: `new-${Date.now()}`, number: '', type: 'Mobile' }]);
    };

    const removePhone = (index: number) => {
        if (phones.length <= 1) {
            Swal.fire({
                icon: 'warning',
                title: 'Atenção',
                text: 'É necessário ter pelo menos um telefone.',
                confirmButtonColor: '#006B3F',
            });
            return;
        }
        setPhones(phones.filter((_, i) => i !== index));
    };

    const handlePhotoChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (file) {
            const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
            if (!allowedTypes.includes(file.type)) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Formato inválido',
                    text: 'Use imagens nos formatos: JPG, PNG, GIF ou WebP',
                    confirmButtonColor: '#006B3F',
                });
                return;
            }
            if (file.size > 5 * 1024 * 1024) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Arquivo muito grande',
                    text: 'O tamanho máximo permitido é 5MB',
                    confirmButtonColor: '#006B3F',
                });
                return;
            }
            setPhotoFile(file);
            setPhotoPreview(URL.createObjectURL(file));
        }
    };

    const handleRemovePhoto = async () => {
        const confirm = await Swal.fire({
            icon: 'question',
            title: 'Remover foto?',
            text: 'Deseja realmente remover a foto deste funcionário?',
            showCancelButton: true,
            confirmButtonText: 'Sim, remover',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#e7515a',
            cancelButtonColor: '#6b7280',
        });

        if (confirm.isConfirmed) {
            try {
                await deletePhoto(id!);
                setCurrentPhotoUrl(null);
                setPhotoPreview(null);
                setPhotoFile(null);
                Swal.fire({
                    icon: 'success',
                    title: 'Foto removida',
                    text: 'A foto foi removida com sucesso.',
                    confirmButtonColor: '#006B3F',
                });
            } catch (error: any) {
                Swal.fire({
                    icon: 'error',
                    title: 'Erro',
                    text: error.message || 'Erro ao remover foto',
                    confirmButtonColor: '#006B3F',
                });
            }
        }
    };

    const validate = (): boolean => {
        const newErrors: FormErrors = {};

        if (!name.trim()) {
            newErrors.name = 'Nome é obrigatório';
        } else if (name.trim().split(' ').length < 2) {
            newErrors.name = 'Informe nome e sobrenome';
        }

        if (!email.trim()) {
            newErrors.email = 'E-mail é obrigatório';
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
            newErrors.email = 'E-mail inválido';
        }

        const cpfNumbers = documentNumber.replace(/\D/g, '');
        if (!cpfNumbers) {
            newErrors.documentNumber = 'CPF é obrigatório';
        } else if (cpfNumbers.length !== 11) {
            newErrors.documentNumber = 'CPF deve ter 11 dígitos';
        }

        if (!birthDate) {
            newErrors.birthDate = 'Data de nascimento é obrigatória';
        } else {
            const birth = new Date(birthDate);
            const today = new Date();
            let age = today.getFullYear() - birth.getFullYear();
            const monthDiff = today.getMonth() - birth.getMonth();
            if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birth.getDate())) {
                age--;
            }
            if (age < 18) {
                newErrors.birthDate = 'Funcionário deve ter 18 anos ou mais';
            }
        }

        if (!roleId) {
            newErrors.roleId = 'Cargo é obrigatório';
        }

        // Verificar telefones
        const validPhones = phones.filter(p => p.number.replace(/\D/g, '').length >= 10);
        if (validPhones.length === 0) {
            newErrors.phones = 'Informe pelo menos um telefone válido';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleResetPassword = async () => {
        const confirm = await Swal.fire({
            icon: 'warning',
            title: 'Resetar Senha',
            text: `Deseja resetar a senha de ${employee?.name}? Uma nova senha temporária será gerada.`,
            showCancelButton: true,
            confirmButtonText: 'Sim, resetar',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#0e1726',
            cancelButtonColor: '#64748b',
        });

        if (confirm.isConfirmed) {
            try {
                const tempPassword = await resetPassword(id!);
                
                await Swal.fire({
                    icon: 'success',
                    title: 'Senha Resetada!',
                    html: `
                        <p class="mb-3">A nova senha temporária é:</p>
                        <div class="bg-slate-100 p-3 rounded-lg font-mono text-lg font-bold text-slate-800 select-all">
                            ${tempPassword}
                        </div>
                        <p class="mt-3 text-sm text-slate-500">
                            Anote esta senha e informe ao funcionário. 
                            Ele deverá alterá-la no primeiro acesso.
                        </p>
                    `,
                    confirmButtonText: 'Entendi',
                    confirmButtonColor: '#006B3F',
                });
            } catch (error: any) {
                Swal.fire({
                    icon: 'error',
                    title: 'Erro',
                    text: error.message || 'Erro ao resetar senha',
                    confirmButtonColor: '#006B3F',
                });
            }
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!validate()) {
            return;
        }

        setSaving(true);

        try {
            const request: UpdateEmployeeRequest = {
                name: name.trim(),
                email: email.trim(),
                documentNumber: documentNumber.replace(/\D/g, ''),
                birthDate: birthDate,
                roleId: roleId,
                managerId: managerId || null,
                phones: phones
                    .filter(p => p.number.replace(/\D/g, '').length >= 10)
                    .map(p => ({
                        number: p.number.replace(/\D/g, ''),
                        type: p.type,
                    })),
            };

            await updateEmployee(id!, request);

            // Upload da foto se houver nova foto selecionada
            if (photoFile) {
                try {
                    await uploadPhoto(id!, photoFile);
                } catch (photoError: any) {
                    console.error('Erro ao fazer upload da foto:', photoError);
                }
            }

            await Swal.fire({
                icon: 'success',
                title: 'Sucesso!',
                text: 'Funcionário atualizado com sucesso.',
                confirmButtonText: 'OK',
                confirmButtonColor: '#006B3F',
            });

            navigate('/employees');
        } catch (error: any) {
            Swal.fire({
                icon: 'error',
                title: 'Erro ao salvar',
                text: error.message || 'Erro ao atualizar funcionário',
                confirmButtonColor: '#006B3F',
            });
        } finally {
            setSaving(false);
        }
    };

    if (loading) {
        return (
            <div className="flex items-center justify-center min-h-[400px]">
                <div className="animate-spin rounded-full h-12 w-12 border-4 border-primary border-t-transparent"></div>
            </div>
        );
    }

    return (
        <div>
            <Breadcrumb 
                items={[
                    { label: 'Gestão de Usuários', path: '/employees' },
                    { label: 'Funcionários', path: '/employees' },
                    { label: 'Editar Funcionário' }
                ]} 
            />

            <div className="panel">
                <div className="mb-6">
                    <h5 className="text-lg font-semibold text-slate-900">Editar Funcionário</h5>
                    <p className="text-sm text-slate-500">{employee?.name}</p>
                </div>

            <form onSubmit={handleSubmit} className="space-y-6" noValidate>
                {/* Foto de Perfil */}
                <div className="border-b border-slate-200 pb-6">
                    <h6 className="text-base font-medium text-slate-900 mb-4">Foto de Perfil</h6>
                    <div className="flex items-center gap-6">
                        <div className="relative">
                            {(photoPreview || currentPhotoUrl) ? (
                                <img
                                    src={photoPreview || currentPhotoUrl || ''}
                                    alt="Foto do funcionário"
                                    className="w-24 h-24 rounded-full object-cover border-2 border-slate-200"
                                />
                            ) : (
                                <div className="w-24 h-24 rounded-full bg-slate-100 flex items-center justify-center border-2 border-dashed border-slate-300">
                                    <svg className="w-8 h-8 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                                    </svg>
                                </div>
                            )}
                        </div>
                        <div className="flex flex-col gap-2">
                            <label className="btn btn-outline-dark btn-sm cursor-pointer">
                                <input
                                    type="file"
                                    accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
                                    onChange={handlePhotoChange}
                                    className="hidden"
                                />
                                {(photoPreview || currentPhotoUrl) ? 'Alterar Foto' : 'Escolher Foto'}
                            </label>
                            {(photoPreview || currentPhotoUrl) && (
                                <button
                                    type="button"
                                    onClick={handleRemovePhoto}
                                    className="btn btn-outline-danger btn-sm"
                                >
                                    Remover
                                </button>
                            )}
                            <p className="text-xs text-slate-500">JPG, PNG, GIF ou WebP. Máx 5MB.</p>
                        </div>
                    </div>
                </div>

                {/* Informações Pessoais */}
                <div className="border-b border-slate-200 pb-6">
                    <h6 className="text-base font-medium text-slate-900 mb-4">Informações Pessoais</h6>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label htmlFor="name" className="block text-sm font-medium text-slate-700 mb-1">
                                Nome Completo <span className="text-red-500">*</span>
                            </label>
                            <input
                                id="name"
                                type="text"
                                className={`form-input ${errors.name ? 'border-red-500' : ''}`}
                                placeholder="Nome e sobrenome"
                                value={name}
                                onChange={(e) => setName(e.target.value)}
                            />
                            {errors.name && <p className="text-red-500 text-xs mt-1">{errors.name}</p>}
                        </div>

                        <div>
                            <label htmlFor="email" className="block text-sm font-medium text-slate-700 mb-1">
                                E-mail <span className="text-red-500">*</span>
                            </label>
                            <input
                                id="email"
                                type="email"
                                className={`form-input ${errors.email ? 'border-red-500' : ''}`}
                                placeholder="email@empresa.com"
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                            />
                            {errors.email && <p className="text-red-500 text-xs mt-1">{errors.email}</p>}
                        </div>

                        <div>
                            <label htmlFor="documentNumber" className="block text-sm font-medium text-slate-700 mb-1">
                                CPF <span className="text-red-500">*</span>
                            </label>
                            <input
                                id="documentNumber"
                                type="text"
                                className={`form-input ${errors.documentNumber ? 'border-red-500' : ''}`}
                                placeholder="000.000.000-00"
                                value={documentNumber}
                                onChange={(e) => handleCPFChange(e.target.value)}
                            />
                            {errors.documentNumber && (
                                <p className="text-red-500 text-xs mt-1">{errors.documentNumber}</p>
                            )}
                        </div>

                        <div>
                            <label htmlFor="birthDate" className="block text-sm font-medium text-slate-700 mb-1">
                                Data de Nascimento <span className="text-red-500">*</span>
                            </label>
                            <input
                                id="birthDate"
                                type="date"
                                className={`form-input ${errors.birthDate ? 'border-red-500' : ''}`}
                                value={birthDate}
                                onChange={(e) => setBirthDate(e.target.value)}
                            />
                            {errors.birthDate && <p className="text-red-500 text-xs mt-1">{errors.birthDate}</p>}
                        </div>
                    </div>
                </div>

                {/* Cargo e Gerente */}
                <div className="border-b border-slate-200 pb-6">
                    <h6 className="text-base font-medium text-slate-900 mb-4">Cargo e Hierarquia</h6>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label htmlFor="roleId" className="block text-sm font-medium text-slate-700 mb-1">
                                Cargo <span className="text-red-500">*</span>
                            </label>
                            <select
                                id="roleId"
                                className={`form-select ${errors.roleId ? 'border-red-500' : ''}`}
                                value={roleId}
                                onChange={(e) => setRoleId(e.target.value)}
                            >
                                <option value="">Selecione o cargo</option>
                                {roles.map((role) => (
                                    <option key={role.id} value={role.id}>
                                        {role.name}
                                    </option>
                                ))}
                                {/* Mostrar cargo atual se não estiver na lista */}
                                {employee && !roles.find(r => r.id === employee.role.id) && (
                                    <option value={employee.role.id}>
                                        {employee.role.name} (atual)
                                    </option>
                                )}
                            </select>
                            {errors.roleId && <p className="text-red-500 text-xs mt-1">{errors.roleId}</p>}
                            <p className="text-xs text-slate-500 mt-1">
                                Apenas cargos de nível inferior ao seu são exibidos
                            </p>
                        </div>

                        <div>
                            <label htmlFor="managerId" className="block text-sm font-medium text-slate-700 mb-1">
                                Gerente
                            </label>
                            <select
                                id="managerId"
                                className="form-select"
                                value={managerId}
                                onChange={(e) => setManagerId(e.target.value)}
                            >
                                <option value="">Sem gerente</option>
                                {managers.map((manager) => (
                                    <option key={manager.id} value={manager.id}>
                                        {manager.name} ({manager.roleName})
                                    </option>
                                ))}
                            </select>
                        </div>
                    </div>
                </div>

                {/* Telefones */}
                <div className="border-b border-slate-200 pb-6">
                    <div className="flex items-center justify-between mb-4">
                        <h6 className="text-base font-medium text-slate-900">
                            Telefones <span className="text-red-500">*</span>
                        </h6>
                        <button
                            type="button"
                            onClick={addPhone}
                            className="btn btn-sm btn-outline-dark flex items-center gap-1"
                        >
                            <IconPlus className="w-4 h-4" />
                            <span>Adicionar</span>
                        </button>
                    </div>
                    
                    {errors.phones && (
                        <p className="text-red-500 text-xs mb-3">{errors.phones}</p>
                    )}

                    <div className="space-y-3">
                        {phones.map((phone, index) => (
                            <div key={phone.id} className="flex gap-3 items-start">
                                <div className="flex-1">
                                    <input
                                        type="text"
                                        className="form-input"
                                        placeholder="(00) 00000-0000"
                                        value={phone.number}
                                        onChange={(e) => handlePhoneChange(index, e.target.value)}
                                    />
                                </div>
                                <div className="w-32">
                                    <select
                                        className="form-select"
                                        value={phone.type}
                                        onChange={(e) => handlePhoneTypeChange(index, e.target.value)}
                                    >
                                        <option value="Mobile">Celular</option>
                                        <option value="Home">Residencial</option>
                                        <option value="Work">Trabalho</option>
                                    </select>
                                </div>
                                <button
                                    type="button"
                                    onClick={() => removePhone(index)}
                                    className="btn btn-sm btn-outline-dark"
                                    title="Remover telefone"
                                >
                                    <IconTrash className="w-4 h-4" />
                                </button>
                            </div>
                        ))}
                    </div>
                </div>

                {/* Segurança */}
                <div className="border-b border-slate-200 pb-6">
                    <h6 className="text-base font-medium text-slate-900 mb-4">Segurança</h6>
                    <div className="flex items-center justify-between p-4 bg-slate-50 rounded-lg border border-slate-200">
                        <div>
                            <p className="font-medium text-slate-700">Resetar Senha</p>
                            <p className="text-sm text-slate-500">
                                Gera uma nova senha temporária para o funcionário
                            </p>
                        </div>
                        <button
                            type="button"
                            onClick={handleResetPassword}
                            className="btn btn-outline-dark"
                        >
                            Resetar Senha
                        </button>
                    </div>
                </div>

                {/* Status */}
                <div className="flex items-center gap-4 text-sm text-slate-500">
                    <span>
                        Status: {employee?.enabled ? (
                            <span className="text-green-600 font-medium">Ativo</span>
                        ) : (
                            <span className="text-amber-600 font-medium">Pendente de aprovação</span>
                        )}
                    </span>
                    {employee?.approvedAt && (
                        <span>
                            • Aprovado por {employee.approvedByName} em{' '}
                            {new Date(employee.approvedAt).toLocaleDateString('pt-BR')}
                        </span>
                    )}
                </div>

                {/* Botões */}
                <div className="flex justify-end gap-3 pt-4 border-t border-slate-200">
                    <button type="button" onClick={() => navigate('/employees')} className="btn btn-outline-dark">
                        Cancelar
                    </button>
                    <button
                        type="submit"
                        className="btn btn-primary"
                        disabled={saving}
                    >
                        {saving ? (
                            <span className="flex items-center gap-2">
                                <svg className="animate-spin h-5 w-5" fill="none" viewBox="0 0 24 24">
                                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                                </svg>
                                <span>Salvando...</span>
                            </span>
                        ) : (
                            'Salvar Alterações'
                        )}
                    </button>
                </div>
            </form>
            </div>
        </div>
    );
};

export default EmployeeEdit;

