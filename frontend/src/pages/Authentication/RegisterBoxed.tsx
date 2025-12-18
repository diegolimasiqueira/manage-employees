import { Link, useNavigate } from 'react-router-dom';
import { useDispatch } from 'react-redux';
import { useEffect, useState } from 'react';
import { setPageTitle } from '../../store/themeConfigSlice';
import IconUser from '../../components/Icon/IconUser';
import IconMail from '../../components/Icon/IconMail';
import IconLockDots from '../../components/Icon/IconLockDots';
import IconPhone from '../../components/Icon/IconPhone';
import IconPlus from '../../components/Icon/IconPlus';
import IconX from '../../components/Icon/IconX';
import { getAvailableManagers, selfRegister, ManagerOption } from '../../services/api';
import Swal from 'sweetalert2';

// Tipos de telefone disponíveis
const PHONE_TYPES = [
    { value: 'Mobile', label: 'Celular' },
    { value: 'Home', label: 'Residencial' },
    { value: 'Work', label: 'Comercial' },
];

// ID do cargo "Funcionário" - usado como padrão para auto-registro
const FUNCIONARIO_ROLE_ID = '33333333-3333-3333-3333-333333333333';

interface PhoneInput {
    id: string;
    number: string;
    type: string;
}

interface FieldErrors {
    name?: string;
    email?: string;
    documentNumber?: string;
    birthDate?: string;
    managerId?: string;
    phones?: string;
    password?: string;
    confirmPassword?: string;
    acceptTerms?: string;
}

const RegisterBoxed = () => {
    const dispatch = useDispatch();
    const navigate = useNavigate();

    // Campos do formulário
    const [name, setName] = useState('');
    const [email, setEmail] = useState('');
    const [documentNumber, setDocumentNumber] = useState('');
    const [birthDate, setBirthDate] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [managerId, setManagerId] = useState('');
    const [phones, setPhones] = useState<PhoneInput[]>([
        { id: '1', number: '', type: 'Mobile' }
    ]);
    const [acceptTerms, setAcceptTerms] = useState(false);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});
    
    // Dados da API
    const [managers, setManagers] = useState<ManagerOption[]>([]);
    const [loadingManagers, setLoadingManagers] = useState(true);

    useEffect(() => {
        dispatch(setPageTitle('Cadastrar'));
        
        // Carregar gerentes da API
        const loadManagers = async () => {
            try {
                const data = await getAvailableManagers();
                setManagers(data);
            } catch (err) {
                console.error('Erro ao carregar gerentes:', err);
                setError('Erro ao carregar lista de gerentes. Tente novamente.');
            } finally {
                setLoadingManagers(false);
            }
        };
        
        loadManagers();
    }, [dispatch]);

    // Validação de idade mínima (18 anos)
    const isAtLeast18 = (date: string): boolean => {
        const birthDateObj = new Date(date);
        const today = new Date();
        const age = today.getFullYear() - birthDateObj.getFullYear();
        const monthDiff = today.getMonth() - birthDateObj.getMonth();
        if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDateObj.getDate())) {
            return age - 1 >= 18;
        }
        return age >= 18;
    };

    // Formatar CPF
    const formatCPF = (value: string): string => {
        const numbers = value.replace(/\D/g, '');
        if (numbers.length <= 3) return numbers;
        if (numbers.length <= 6) return `${numbers.slice(0, 3)}.${numbers.slice(3)}`;
        if (numbers.length <= 9) return `${numbers.slice(0, 3)}.${numbers.slice(3, 6)}.${numbers.slice(6)}`;
        return `${numbers.slice(0, 3)}.${numbers.slice(3, 6)}.${numbers.slice(6, 9)}-${numbers.slice(9, 11)}`;
    };

    // Formatar telefone
    const formatPhone = (value: string): string => {
        const numbers = value.replace(/\D/g, '');
        if (numbers.length <= 2) return numbers;
        if (numbers.length <= 7) return `(${numbers.slice(0, 2)}) ${numbers.slice(2)}`;
        if (numbers.length <= 11) return `(${numbers.slice(0, 2)}) ${numbers.slice(2, 7)}-${numbers.slice(7)}`;
        return `(${numbers.slice(0, 2)}) ${numbers.slice(2, 7)}-${numbers.slice(7, 11)}`;
    };

    const handleDocumentChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const formatted = formatCPF(e.target.value);
        setDocumentNumber(formatted);
        if (fieldErrors.documentNumber) {
            setFieldErrors(prev => ({ ...prev, documentNumber: undefined }));
        }
    };

    // Gerenciar telefones
    const handlePhoneChange = (id: string, field: 'number' | 'type', value: string) => {
        setPhones(prev => prev.map(phone => {
            if (phone.id === id) {
                if (field === 'number') {
                    return { ...phone, number: formatPhone(value) };
                }
                return { ...phone, [field]: value };
            }
            return phone;
        }));
        if (fieldErrors.phones) {
            setFieldErrors(prev => ({ ...prev, phones: undefined }));
        }
    };

    const addPhone = () => {
        const newId = (phones.length + 1).toString();
        setPhones(prev => [...prev, { id: newId, number: '', type: 'Mobile' }]);
    };

    const removePhone = (id: string) => {
        if (phones.length > 1) {
            setPhones(prev => prev.filter(phone => phone.id !== id));
        }
    };

    // Limpar erro do campo ao digitar
    const clearFieldError = (field: keyof FieldErrors) => {
        if (fieldErrors[field]) {
            setFieldErrors(prev => ({ ...prev, [field]: undefined }));
        }
    };

    const validateForm = (): boolean => {
        const errors: FieldErrors = {};
        let isValid = true;

        // Nome completo
        if (!name.trim()) {
            errors.name = 'Nome completo é obrigatório';
            isValid = false;
        } else if (name.trim().split(' ').length < 2) {
            errors.name = 'Informe nome e sobrenome';
            isValid = false;
        }

        // E-mail
        if (!email.trim()) {
            errors.email = 'E-mail é obrigatório';
            isValid = false;
        } else if (!email.includes('@') || !email.includes('.')) {
            errors.email = 'E-mail inválido';
            isValid = false;
        }

        // CPF
        if (!documentNumber.trim()) {
            errors.documentNumber = 'CPF é obrigatório';
            isValid = false;
        } else if (documentNumber.replace(/\D/g, '').length !== 11) {
            errors.documentNumber = 'CPF deve conter 11 dígitos';
            isValid = false;
        }

        // Data de nascimento
        if (!birthDate) {
            errors.birthDate = 'Data de nascimento é obrigatória';
            isValid = false;
        } else if (!isAtLeast18(birthDate)) {
            errors.birthDate = 'Você deve ter pelo menos 18 anos';
            isValid = false;
        }

        // Gerente
        if (!managerId) {
            errors.managerId = 'Selecione um gerente';
            isValid = false;
        }

        // Telefones
        const validPhones = phones.filter(p => p.number.replace(/\D/g, '').length >= 10);
        if (validPhones.length === 0) {
            errors.phones = 'Informe pelo menos um telefone válido';
            isValid = false;
        }

        // Senha
        if (!password) {
            errors.password = 'Senha é obrigatória';
            isValid = false;
        } else if (password.length < 8) {
            errors.password = 'Senha deve ter no mínimo 8 caracteres';
            isValid = false;
        } else {
            const hasUppercase = /[A-Z]/.test(password);
            const hasLowercase = /[a-z]/.test(password);
            const hasNumber = /[0-9]/.test(password);
            const hasSpecial = /[!@#$%^&*(),.?":{}|<>]/.test(password);
            
            if (!hasUppercase || !hasLowercase || !hasNumber || !hasSpecial) {
                errors.password = 'Senha fraca: use maiúsculas, minúsculas, números e símbolos';
                isValid = false;
            }
        }

        // Confirmar senha
        if (!confirmPassword) {
            errors.confirmPassword = 'Confirme sua senha';
            isValid = false;
        } else if (password !== confirmPassword) {
            errors.confirmPassword = 'As senhas não conferem';
            isValid = false;
        }

        // Termos
        if (!acceptTerms) {
            errors.acceptTerms = 'Você deve aceitar os termos de uso';
            isValid = false;
        }

        setFieldErrors(errors);
        
        // Se houver erros, mostrar o primeiro como erro geral
        if (!isValid) {
            const firstError = Object.values(errors)[0];
            setError(firstError || 'Preencha todos os campos obrigatórios');
        }

        return isValid;
    };

    const submitForm = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');

        if (!validateForm()) return;

        setLoading(true);

        const validPhones = phones
            .filter(p => p.number.replace(/\D/g, '').length >= 10)
            .map(p => ({
                number: p.number.replace(/\D/g, ''), // Enviar apenas números
                type: p.type
            }));

        try {
            await selfRegister({
                name: name.trim(),
                email: email.trim().toLowerCase(),
                documentNumber: documentNumber.replace(/\D/g, ''),
                password,
                confirmPassword,
                birthDate,
                roleId: FUNCIONARIO_ROLE_ID, // Cargo padrão para auto-registro
                managerId,
                phones: validPhones
            });

            // Mostrar modal de sucesso
            await Swal.fire({
                icon: 'success',
                title: 'Cadastro Realizado!',
                html: `
                    <div class="text-left">
                        <p class="mb-3">Seu cadastro foi enviado com sucesso!</p>
                        <div class="bg-blue-50 border border-blue-200 rounded-lg p-3 text-sm">
                            <p class="font-semibold text-blue-800 mb-1">📋 Próximos passos:</p>
                            <ul class="text-blue-700 list-disc list-inside space-y-1">
                                <li>Aguarde a aprovação de um administrador</li>
                                <li>Você receberá uma notificação quando aprovado</li>
                                <li>Após aprovação, faça login com seu e-mail e senha</li>
                            </ul>
                        </div>
                    </div>
                `,
                confirmButtonText: 'Ir para Login',
                confirmButtonColor: '#006B3F',
                allowOutsideClick: false,
                customClass: {
                    popup: 'rounded-2xl',
                    title: 'text-xl font-bold text-slate-800',
                    confirmButton: 'rounded-lg px-6 py-2'
                }
            });

            navigate('/auth/boxed-signin');
        } catch (err: any) {
            // Mostrar modal de erro
            Swal.fire({
                icon: 'error',
                title: 'Erro no Cadastro',
                text: err.message || 'Erro ao realizar cadastro. Tente novamente.',
                confirmButtonText: 'Tentar Novamente',
                confirmButtonColor: '#dc2626',
                customClass: {
                    popup: 'rounded-2xl',
                    title: 'text-xl font-bold text-slate-800',
                    confirmButton: 'rounded-lg px-6 py-2'
                }
            });
            setError(err.message || 'Erro ao realizar cadastro. Tente novamente.');
        } finally {
            setLoading(false);
        }
    };

    // Classe de erro para inputs
    const inputErrorClass = (field: keyof FieldErrors) => 
        fieldErrors[field] ? 'border-red-300 focus:border-red-500' : 'border-slate-200 focus:border-primary';

    return (
        <div className="relative min-h-screen overflow-hidden">
            {/* Fundo Glassmorphism + Mesh Gradient */}
            <div className="absolute inset-0 bg-gradient-to-br from-primary-light via-[#F0F7FF] to-white">
                <div 
                    className="absolute top-0 right-0 w-[700px] h-[700px] rounded-full opacity-30 blur-[120px]"
                    style={{ background: 'radial-gradient(circle, #006B3F 0%, transparent 70%)' }}
                ></div>
                <div 
                    className="absolute top-1/4 left-0 w-[600px] h-[600px] rounded-full opacity-25 blur-[100px]"
                    style={{ background: 'radial-gradient(circle, #FFCC00 0%, transparent 70%)' }}
                ></div>
                <div 
                    className="absolute bottom-0 left-1/3 w-[650px] h-[650px] rounded-full opacity-35 blur-[90px]"
                    style={{ background: 'radial-gradient(circle, #C9FBE2 0%, transparent 70%)' }}
                ></div>
                <div 
                    className="absolute bottom-1/4 right-1/4 w-[500px] h-[500px] rounded-full opacity-20 blur-[100px]"
                    style={{ background: 'radial-gradient(circle, #FFF9E6 0%, transparent 70%)' }}
                ></div>
                <svg className="absolute inset-0 w-full h-full opacity-[0.06]" xmlns="http://www.w3.org/2000/svg">
                    <path d="M0,100 Q400,50 800,100 T1600,100" stroke="#006B3F" strokeWidth="2" fill="none" />
                    <path d="M0,300 Q500,250 1000,300 T2000,300" stroke="#FFCC00" strokeWidth="2" fill="none" />
                    <path d="M0,500 Q600,450 1200,500 T2400,500" stroke="#C9FBE2" strokeWidth="1.5" fill="none" />
                </svg>
                <div 
                    className="absolute inset-0 opacity-[0.02]"
                    style={{ backgroundImage: 'radial-gradient(circle, #006B3F 1px, transparent 1px)', backgroundSize: '40px 40px' }}
                ></div>
            </div>
            
            <div className="relative flex min-h-screen items-center justify-center px-4 py-4 sm:px-8">
                
                {/* Elementos decorativos flutuantes */}
                <div className="absolute top-20 left-10 w-32 h-32 rounded-full bg-gradient-to-br from-primary/20 to-secondary/20 backdrop-blur-sm border border-white/30 hidden lg:block animate-float"></div>
                <div className="absolute bottom-32 right-20 w-24 h-24 rounded-full bg-gradient-to-br from-secondary/20 to-primary/20 backdrop-blur-sm border border-white/30 hidden lg:block animate-float-delayed"></div>
                <div className="absolute top-1/3 right-10 w-20 h-20 rounded-lg rotate-45 bg-gradient-to-br from-primary/15 to-secondary/15 backdrop-blur-sm border border-white/20 hidden lg:block animate-float-slow"></div>
                
                {/* Container Principal */}
                <div className="relative flex w-full max-w-[1100px] flex-col justify-between overflow-hidden rounded-2xl bg-white/80 backdrop-blur-xl shadow-[0_8px_32px_rgba(0,0,0,0.08)] border border-white/20 lg:flex-row">
                    
                    {/* Lado Esquerdo - Brand Section */}
                    <div className="relative hidden w-full items-center justify-center bg-[#0A3D2A] p-12 lg:flex lg:w-[35%] overflow-hidden">
                        <div className="absolute inset-0 opacity-[0.05]" style={{
                            backgroundImage: 'radial-gradient(circle, #FFCC00 1px, transparent 1px)',
                            backgroundSize: '24px 24px'
                        }}></div>
                        <div className="absolute top-0 right-0 w-[600px] h-[600px] bg-gradient-to-br from-primary/20 to-transparent rounded-full blur-3xl"></div>
                        <div className="absolute bottom-0 left-0 w-[400px] h-[400px] bg-gradient-to-tr from-secondary/10 to-transparent rounded-full blur-3xl"></div>
                        
                        <div className="relative z-10 flex flex-col items-center justify-center h-full">
                            <div className="mb-8">
                                <div className="w-20 h-20 rounded-2xl bg-white/10 backdrop-blur-sm flex items-center justify-center border border-white/20">
                                    <svg className="w-10 h-10 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z" />
                                    </svg>
                                </div>
                            </div>
                            <div className="text-center">
                                <h2 className="text-white text-2xl font-bold mb-3">Criar Conta</h2>
                                <p className="text-white/70 text-sm max-w-[280px]">
                                    Preencha seus dados para solicitar acesso ao sistema
                                </p>
                            </div>
                            <div className="mt-12 flex gap-2">
                                <div className="w-2 h-2 rounded-full bg-white/40"></div>
                                <div className="w-2 h-2 rounded-full bg-secondary"></div>
                                <div className="w-2 h-2 rounded-full bg-white/40"></div>
                            </div>
                        </div>
                    </div>
                    
                    {/* Lado Direito - Formulário */}
                    <div className="relative flex w-full flex-col items-center justify-center px-4 py-6 sm:px-8 lg:w-[65%]">
                        
                        {/* Logo Mobile */}
                        <div className="flex w-full max-w-[520px] items-center justify-center mb-4 lg:hidden">
                            <div className="w-12 h-12 rounded-xl bg-primary flex items-center justify-center">
                                <svg className="w-6 h-6 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z" />
                                </svg>
                            </div>
                        </div>
                        
                        {/* Formulário */}
                        <div className="w-full max-w-[520px]">
                            <div className="mb-4">
                                <h1 className="text-xl font-bold !leading-tight text-slate-900 mb-1">
                                    Cadastre-se
                                </h1>
                                <p className="text-xs text-slate-500">
                                    Preencha os dados abaixo para solicitar seu cadastro
                                </p>
                            </div>

                            {/* Mensagem de erro geral */}
                            {error && (
                                <div className="mb-3 rounded-lg border border-red-200 bg-red-50 p-3 flex items-start gap-2">
                                    <svg className="w-4 h-4 text-red-600 flex-shrink-0 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                                    </svg>
                                    <p className="text-xs text-red-800">{error}</p>
                                </div>
                            )}

                            <form className="space-y-3" onSubmit={submitForm} noValidate>
                                {/* Nome Completo */}
                                <div>
                                    <label htmlFor="Name" className="block text-xs font-medium text-slate-700 mb-1">
                                        Nome Completo <span className="text-red-500">*</span>
                                    </label>
                                    <div className="relative">
                                        <input
                                            id="Name"
                                            type="text"
                                            placeholder="Ex: João da Silva"
                                            className={`form-input rounded-lg ps-10 placeholder:text-slate-400 h-9 ${inputErrorClass('name')}`}
                                            value={name}
                                            onChange={(e) => { setName(e.target.value); clearFieldError('name'); }}
                                        />
                                        <span className="absolute start-3 top-1/2 -translate-y-1/2 text-slate-400">
                                            <IconUser className="w-5 h-5" fill={true} />
                                        </span>
                                    </div>
                                    {fieldErrors.name && <p className="text-xs text-red-500 mt-0.5">{fieldErrors.name}</p>}
                                </div>

                                {/* E-mail */}
                                <div>
                                    <label htmlFor="Email" className="block text-xs font-medium text-slate-700 mb-1">
                                        E-mail <span className="text-red-500">*</span>
                                    </label>
                                    <div className="relative">
                                        <input
                                            id="Email"
                                            type="email"
                                            placeholder="seu.email@empresa.com"
                                            className={`form-input rounded-lg ps-10 placeholder:text-slate-400 h-9 ${inputErrorClass('email')}`}
                                            value={email}
                                            onChange={(e) => { setEmail(e.target.value); clearFieldError('email'); }}
                                        />
                                        <span className="absolute start-3 top-1/2 -translate-y-1/2 text-slate-400">
                                            <IconMail className="w-5 h-5" fill={true} />
                                        </span>
                                    </div>
                                    {fieldErrors.email && <p className="text-xs text-red-500 mt-0.5">{fieldErrors.email}</p>}
                                </div>

                                {/* CPF e Data de Nascimento */}
                                <div className="grid grid-cols-2 gap-3">
                                    <div>
                                        <label htmlFor="Document" className="block text-xs font-medium text-slate-700 mb-1">
                                            CPF <span className="text-red-500">*</span>
                                        </label>
                                        <input
                                            id="Document"
                                            type="text"
                                            placeholder="000.000.000-00"
                                            className={`form-input rounded-lg placeholder:text-slate-400 h-9 ${inputErrorClass('documentNumber')}`}
                                            value={documentNumber}
                                            onChange={handleDocumentChange}
                                            maxLength={14}
                                        />
                                        {fieldErrors.documentNumber && <p className="text-xs text-red-500 mt-0.5">{fieldErrors.documentNumber}</p>}
                                    </div>
                                    <div>
                                        <label htmlFor="BirthDate" className="block text-xs font-medium text-slate-700 mb-1">
                                            Data de Nascimento <span className="text-red-500">*</span>
                                        </label>
                                        <input
                                            id="BirthDate"
                                            type="date"
                                            className={`form-input rounded-lg placeholder:text-slate-400 h-9 ${inputErrorClass('birthDate')}`}
                                            value={birthDate}
                                            onChange={(e) => { setBirthDate(e.target.value); clearFieldError('birthDate'); }}
                                            max={new Date(new Date().setFullYear(new Date().getFullYear() - 18)).toISOString().split('T')[0]}
                                        />
                                        {fieldErrors.birthDate && <p className="text-xs text-red-500 mt-0.5">{fieldErrors.birthDate}</p>}
                                    </div>
                                </div>

                                {/* Nome do Gerente - Combobox */}
                                <div>
                                    <label htmlFor="Manager" className="block text-xs font-medium text-slate-700 mb-1">
                                        Nome do Gerente <span className="text-red-500">*</span>
                                    </label>
                                    <select
                                        id="Manager"
                                        className={`form-select rounded-lg h-9 ${inputErrorClass('managerId')}`}
                                        value={managerId}
                                        onChange={(e) => { setManagerId(e.target.value); clearFieldError('managerId'); }}
                                        disabled={loadingManagers}
                                    >
                                        <option value="">{loadingManagers ? 'Carregando...' : 'Selecione seu gerente'}</option>
                                        {managers.map(manager => (
                                            <option key={manager.id} value={manager.id}>
                                                {manager.name} - {manager.roleName}
                                            </option>
                                        ))}
                                    </select>
                                    {fieldErrors.managerId && <p className="text-xs text-red-500 mt-0.5">{fieldErrors.managerId}</p>}
                                </div>

                                {/* Telefones */}
                                <div>
                                    <div className="flex items-center justify-between mb-1">
                                        <label className="block text-xs font-medium text-slate-700">
                                            Telefones <span className="text-red-500">*</span>
                                        </label>
                                        <button
                                            type="button"
                                            onClick={addPhone}
                                            className="text-xs text-primary hover:text-primary/80 font-medium flex items-center gap-1"
                                        >
                                            <IconPlus className="w-4 h-4" />
                                            Adicionar
                                        </button>
                                    </div>
                                    <div className="space-y-2">
                                        {phones.map((phone, index) => (
                                            <div key={phone.id} className="flex gap-2 items-center">
                                                <div className="flex-1 relative">
                                                    <input
                                                        type="tel"
                                                        placeholder="(00) 00000-0000"
                                                        className={`form-input rounded-lg ps-10 placeholder:text-slate-400 h-9 w-full ${fieldErrors.phones && index === 0 ? 'border-red-300' : 'border-slate-200 focus:border-primary'}`}
                                                        value={phone.number}
                                                        onChange={(e) => handlePhoneChange(phone.id, 'number', e.target.value)}
                                                        maxLength={15}
                                                    />
                                                    <span className="absolute start-3 top-1/2 -translate-y-1/2 text-slate-400">
                                                        <IconPhone className="w-5 h-5" />
                                                    </span>
                                                </div>
                                                <select
                                                    className="form-select rounded-lg border-slate-200 focus:border-primary h-9 w-28 text-sm"
                                                    value={phone.type}
                                                    onChange={(e) => handlePhoneChange(phone.id, 'type', e.target.value)}
                                                >
                                                    {PHONE_TYPES.map(type => (
                                                        <option key={type.value} value={type.value}>
                                                            {type.label}
                                                        </option>
                                                    ))}
                                                </select>
                                                {phones.length > 1 && (
                                                    <button
                                                        type="button"
                                                        onClick={() => removePhone(phone.id)}
                                                        className="w-9 h-9 rounded-lg border border-red-200 text-red-500 hover:bg-red-50 flex items-center justify-center"
                                                        title="Remover telefone"
                                                    >
                                                        <IconX className="w-4 h-4" />
                                                    </button>
                                                )}
                                            </div>
                                        ))}
                                    </div>
                                    {fieldErrors.phones && <p className="text-xs text-red-500 mt-0.5">{fieldErrors.phones}</p>}
                                </div>

                                {/* Senha e Confirmação */}
                                <div className="grid grid-cols-2 gap-3">
                                    <div>
                                        <label htmlFor="Password" className="block text-xs font-medium text-slate-700 mb-1">
                                            Senha <span className="text-red-500">*</span>
                                        </label>
                                        <div className="relative">
                                            <input
                                                id="Password"
                                                type="password"
                                                placeholder="Mínimo 8 caracteres"
                                                className={`form-input rounded-lg ps-10 placeholder:text-slate-400 h-9 ${inputErrorClass('password')}`}
                                                value={password}
                                                onChange={(e) => { setPassword(e.target.value); clearFieldError('password'); }}
                                            />
                                            <span className="absolute start-3 top-1/2 -translate-y-1/2 text-slate-400">
                                                <IconLockDots className="w-5 h-5" fill={true} />
                                            </span>
                                        </div>
                                        {fieldErrors.password && <p className="text-xs text-red-500 mt-0.5">{fieldErrors.password}</p>}
                                    </div>
                                    <div>
                                        <label htmlFor="ConfirmPassword" className="block text-xs font-medium text-slate-700 mb-1">
                                            Confirmar Senha <span className="text-red-500">*</span>
                                        </label>
                                        <div className="relative">
                                            <input
                                                id="ConfirmPassword"
                                                type="password"
                                                placeholder="Repita a senha"
                                                className={`form-input rounded-lg ps-10 placeholder:text-slate-400 h-9 ${inputErrorClass('confirmPassword')}`}
                                                value={confirmPassword}
                                                onChange={(e) => { setConfirmPassword(e.target.value); clearFieldError('confirmPassword'); }}
                                            />
                                            <span className="absolute start-3 top-1/2 -translate-y-1/2 text-slate-400">
                                                <IconLockDots className="w-5 h-5" fill={true} />
                                            </span>
                                        </div>
                                        {fieldErrors.confirmPassword && <p className="text-xs text-red-500 mt-0.5">{fieldErrors.confirmPassword}</p>}
                                    </div>
                                </div>

                                {/* Termos de uso */}
                                <div className="pt-1">
                                    <label className="flex cursor-pointer items-center gap-2">
                                        <input
                                            type="checkbox"
                                            className={`form-checkbox w-4 h-4 rounded text-primary focus:ring-primary ${fieldErrors.acceptTerms ? 'border-red-300' : 'border-slate-300'}`}
                                            checked={acceptTerms}
                                            onChange={(e) => { setAcceptTerms(e.target.checked); clearFieldError('acceptTerms'); }}
                                        />
                                        <span className="text-xs text-slate-600">
                                            Li e aceito os{' '}
                                            <Link to="/terms" className="text-primary hover:underline">
                                                termos de uso
                                            </Link>
                                            <span className="text-red-500"> *</span>
                                        </span>
                                    </label>
                                    {fieldErrors.acceptTerms && <p className="text-xs text-red-500 mt-0.5">{fieldErrors.acceptTerms}</p>}
                                </div>

                                {/* Aviso sobre aprovação */}
                                <div className="rounded-lg border border-blue-200 bg-blue-50 p-2 flex items-center gap-2">
                                    <svg className="w-4 h-4 text-blue-600 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                                    </svg>
                                    <p className="text-xs text-blue-800">
                                        Após o cadastro, aguarde aprovação de um administrador.
                                    </p>
                                </div>

                                <button
                                    type="submit"
                                    className="!mt-4 w-full h-9 rounded-lg bg-primary hover:bg-primary/90 text-white font-medium shadow-sm transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
                                    disabled={loading}
                                >
                                    {loading ? (
                                        <span className="flex items-center justify-center gap-2">
                                            <svg className="animate-spin h-5 w-5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                            </svg>
                                            <span>Cadastrando...</span>
                                        </span>
                                    ) : (
                                        'Cadastrar'
                                    )}
                                </button>
                            </form>

                            <div className="text-center mt-4">
                                <p className="text-xs text-slate-600">
                                    Já tem uma conta?{' '}
                                    <Link to="/auth/boxed-signin" className="text-primary hover:text-primary/80 font-semibold">
                                        Entrar
                                    </Link>
                                </p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default RegisterBoxed;
