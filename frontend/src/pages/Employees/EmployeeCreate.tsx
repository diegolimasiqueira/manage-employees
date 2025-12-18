import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch } from 'react-redux';
import { setPageTitle } from '../../store/themeConfigSlice';
import {
    createEmployee,
    getAssignableRoles,
    getManagersForEdit,
    getCurrentUser,
    uploadPhoto,
    RoleOption,
    ManagerOption,
} from '../../services/api';
import Swal from 'sweetalert2';
import Breadcrumb from '../../components/Breadcrumb';
import IconPlus from '../../components/Icon/IconPlus';
import IconTrash from '../../components/Icon/IconTrash';

interface PhoneField {
    id: string;
    number: string;
    type: string;
}

interface FormErrors {
    name?: string;
    email?: string;
    documentNumber?: string;
    password?: string;
    confirmPassword?: string;
    birthDate?: string;
    roleId?: string;
    phones?: string;
}

const EmployeeCreate = () => {
    const dispatch = useDispatch();
    const navigate = useNavigate();

    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [roles, setRoles] = useState<RoleOption[]>([]);
    const [managers, setManagers] = useState<ManagerOption[]>([]);
    const [errors, setErrors] = useState<FormErrors>({});

    // Form fields
    const [name, setName] = useState('');
    const [email, setEmail] = useState('');
    const [documentNumber, setDocumentNumber] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [birthDate, setBirthDate] = useState('');
    const [roleId, setRoleId] = useState('');
    const [managerId, setManagerId] = useState('');
    const [phones, setPhones] = useState<PhoneField[]>([
        { id: 'phone-1', number: '', type: 'Mobile' }
    ]);
    const [photoFile, setPhotoFile] = useState<File | null>(null);
    const [photoPreview, setPhotoPreview] = useState<string | null>(null);

    useEffect(() => {
        dispatch(setPageTitle('Novo Funcionário'));
        
        // Verificar permissão
        const currentUser = getCurrentUser();
        if (!currentUser?.canCreateEmployees) {
            Swal.fire({
                icon: 'warning',
                title: 'Sem Permissão',
                text: 'Você não tem permissão para criar funcionários.',
                confirmButtonColor: '#006B3F',
            }).then(() => navigate('/employees'));
            return;
        }

        loadData();
    }, [dispatch, navigate]);

    const loadData = async () => {
        try {
            setLoading(true);
            
            const [rolesData, managersData] = await Promise.all([
                getAssignableRoles(),
                getManagersForEdit(),
            ]);

            setRoles(rolesData);
            setManagers(managersData);
        } catch (error: any) {
            Swal.fire({
                icon: 'error',
                title: 'Erro',
                text: error.message || 'Erro ao carregar dados',
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
            // Validar extensão
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
            // Validar tamanho (5MB)
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

    const removePhoto = () => {
        setPhotoFile(null);
        setPhotoPreview(null);
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

        if (!password) {
            newErrors.password = 'Senha é obrigatória';
        } else if (password.length < 8) {
            newErrors.password = 'A senha deve ter pelo menos 8 caracteres';
        }

        if (!confirmPassword) {
            newErrors.confirmPassword = 'Confirmação de senha é obrigatória';
        } else if (password !== confirmPassword) {
            newErrors.confirmPassword = 'As senhas não coincidem';
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

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!validate()) {
            return;
        }

        setSaving(true);

        try {
            const newEmployee = await createEmployee({
                name: name.trim(),
                email: email.trim(),
                documentNumber: documentNumber.replace(/\D/g, ''),
                password: password,
                birthDate: birthDate,
                roleId: roleId,
                managerId: managerId || undefined,
                phones: phones
                    .filter(p => p.number.replace(/\D/g, '').length >= 10)
                    .map(p => ({
                        number: p.number.replace(/\D/g, ''),
                        type: p.type,
                    })),
            });

            // Upload da foto se houver
            if (photoFile && newEmployee.id) {
                try {
                    await uploadPhoto(newEmployee.id, photoFile);
                } catch (photoError: any) {
                    console.error('Erro ao fazer upload da foto:', photoError);
                    // Não bloqueia o fluxo, só exibe aviso
                }
            }

            await Swal.fire({
                icon: 'success',
                title: 'Sucesso!',
                text: 'Funcionário criado com sucesso.',
                confirmButtonText: 'OK',
                confirmButtonColor: '#006B3F',
            });

            navigate('/employees');
        } catch (error: any) {
            Swal.fire({
                icon: 'error',
                title: 'Erro ao criar',
                text: error.message || 'Erro ao criar funcionário',
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
                    { label: 'Novo Funcionário' }
                ]} 
            />

            <div className="panel border border-slate-200">
                <div className="mb-6">
                    <h5 className="text-lg font-semibold text-slate-900">Novo Funcionário</h5>
                    <p className="text-sm text-slate-500">Preencha os dados do novo funcionário</p>
                </div>

            <form onSubmit={handleSubmit} className="space-y-6" noValidate>
                {/* Foto de Perfil */}
                <div className="border-b border-slate-200 pb-6">
                    <h6 className="text-base font-medium text-slate-900 mb-4">Foto de Perfil</h6>
                    <div className="flex items-center gap-6">
                        <div className="relative">
                            {photoPreview ? (
                                <img
                                    src={photoPreview}
                                    alt="Preview"
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
                                {photoPreview ? 'Alterar Foto' : 'Escolher Foto'}
                            </label>
                            {photoPreview && (
                                <button
                                    type="button"
                                    onClick={removePhoto}
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
                            <label htmlFor="name" className="block text-xs text-slate-500 uppercase tracking-wide mb-2">
                                Nome Completo <span className="text-red-500">*</span>
                            </label>
                            <input
                                id="name"
                                type="text"
                                className={`form-input border-slate-200 focus:border-primary ${errors.name ? 'border-red-500' : ''}`}
                                placeholder="Nome e sobrenome"
                                value={name}
                                onChange={(e) => setName(e.target.value)}
                            />
                            {errors.name && <p className="text-red-500 text-xs mt-1">{errors.name}</p>}
                        </div>

                        <div>
                            <label htmlFor="email" className="block text-xs text-slate-500 uppercase tracking-wide mb-2">
                                E-mail <span className="text-red-500">*</span>
                            </label>
                            <input
                                id="email"
                                type="email"
                                className={`form-input border-slate-200 focus:border-primary ${errors.email ? 'border-red-500' : ''}`}
                                placeholder="email@empresa.com"
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                            />
                            {errors.email && <p className="text-red-500 text-xs mt-1">{errors.email}</p>}
                        </div>

                        <div>
                            <label htmlFor="documentNumber" className="block text-xs text-slate-500 uppercase tracking-wide mb-2">
                                CPF <span className="text-red-500">*</span>
                            </label>
                            <input
                                id="documentNumber"
                                type="text"
                                className={`form-input border-slate-200 focus:border-primary ${errors.documentNumber ? 'border-red-500' : ''}`}
                                placeholder="000.000.000-00"
                                value={documentNumber}
                                onChange={(e) => handleCPFChange(e.target.value)}
                            />
                            {errors.documentNumber && (
                                <p className="text-red-500 text-xs mt-1">{errors.documentNumber}</p>
                            )}
                        </div>

                        <div>
                            <label htmlFor="birthDate" className="block text-xs text-slate-500 uppercase tracking-wide mb-2">
                                Data de Nascimento <span className="text-red-500">*</span>
                            </label>
                            <input
                                id="birthDate"
                                type="date"
                                className={`form-input border-slate-200 focus:border-primary ${errors.birthDate ? 'border-red-500' : ''}`}
                                value={birthDate}
                                onChange={(e) => setBirthDate(e.target.value)}
                            />
                            {errors.birthDate && <p className="text-red-500 text-xs mt-1">{errors.birthDate}</p>}
                        </div>
                    </div>
                </div>

                {/* Senha */}
                <div className="border-b border-slate-200 pb-6">
                    <h6 className="text-base font-medium text-slate-900 mb-4">Credenciais de Acesso</h6>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label htmlFor="password" className="block text-xs text-slate-500 uppercase tracking-wide mb-2">
                                Senha <span className="text-red-500">*</span>
                            </label>
                            <input
                                id="password"
                                type="password"
                                className={`form-input border-slate-200 focus:border-primary ${errors.password ? 'border-red-500' : ''}`}
                                placeholder="Mínimo 8 caracteres"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                            />
                            {errors.password && <p className="text-red-500 text-xs mt-1">{errors.password}</p>}
                        </div>

                        <div>
                            <label htmlFor="confirmPassword" className="block text-xs text-slate-500 uppercase tracking-wide mb-2">
                                Confirmar Senha <span className="text-red-500">*</span>
                            </label>
                            <input
                                id="confirmPassword"
                                type="password"
                                className={`form-input border-slate-200 focus:border-primary ${errors.confirmPassword ? 'border-red-500' : ''}`}
                                placeholder="Repita a senha"
                                value={confirmPassword}
                                onChange={(e) => setConfirmPassword(e.target.value)}
                            />
                            {errors.confirmPassword && <p className="text-red-500 text-xs mt-1">{errors.confirmPassword}</p>}
                        </div>
                    </div>
                </div>

                {/* Cargo e Gerente */}
                <div className="border-b border-slate-200 pb-6">
                    <h6 className="text-base font-medium text-slate-900 mb-4">Cargo e Hierarquia</h6>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label htmlFor="roleId" className="block text-xs text-slate-500 uppercase tracking-wide mb-2">
                                Cargo <span className="text-red-500">*</span>
                            </label>
                            <select
                                id="roleId"
                                className={`form-select border-slate-200 ${errors.roleId ? 'border-red-500' : ''}`}
                                value={roleId}
                                onChange={(e) => setRoleId(e.target.value)}
                            >
                                <option value="">Selecione o cargo</option>
                                {roles.map((role) => (
                                    <option key={role.id} value={role.id}>
                                        {role.name}
                                    </option>
                                ))}
                            </select>
                            {errors.roleId && <p className="text-red-500 text-xs mt-1">{errors.roleId}</p>}
                            <p className="text-xs text-slate-500 mt-1">
                                Apenas cargos de nível inferior ao seu são exibidos
                            </p>
                        </div>

                        <div>
                            <label htmlFor="managerId" className="block text-xs text-slate-500 uppercase tracking-wide mb-2">
                                Gerente
                            </label>
                            <select
                                id="managerId"
                                className="form-select border-slate-200"
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
                                        className="form-input border-slate-200 focus:border-primary"
                                        placeholder="(00) 00000-0000"
                                        value={phone.number}
                                        onChange={(e) => handlePhoneChange(index, e.target.value)}
                                    />
                                </div>
                                <div className="w-32">
                                    <select
                                        className="form-select border-slate-200"
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
                                <span>Criando...</span>
                            </span>
                        ) : (
                            'Criar Funcionário'
                        )}
                    </button>
                </div>
            </form>
            </div>
        </div>
    );
};

export default EmployeeCreate;

