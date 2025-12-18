import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useDispatch } from 'react-redux';
import { useEffect, useState } from 'react';
import { setPageTitle } from '../../store/themeConfigSlice';
import IconMail from '../../components/Icon/IconMail';
import IconLockDots from '../../components/Icon/IconLockDots';
import Swal from 'sweetalert2';
import { login, isAuthenticated } from '../../services/api';

const LoginBoxed = () => {
    const dispatch = useDispatch();
    const navigate = useNavigate();
    const location = useLocation();
    
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [rememberMe, setRememberMe] = useState(false);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    
    // Mensagem vinda do registro
    const successMessage = (location.state as { successMessage?: string })?.successMessage || '';

    useEffect(() => {
        dispatch(setPageTitle('Entrar'));
        
        // Se já está autenticado, redireciona para home
        if (isAuthenticated()) {
            navigate('/');
            return;
        }
        
        // Mostrar toast de sucesso se veio do registro
        if (successMessage) {
            const Toast = Swal.mixin({
                toast: true,
                position: 'top-end',
                showConfirmButton: false,
                timer: 5000,
                timerProgressBar: true,
                didOpen: (toast) => {
                    toast.onmouseenter = Swal.stopTimer;
                    toast.onmouseleave = Swal.resumeTimer;
                }
            });
            Toast.fire({
                icon: 'info',
                title: 'Cadastro enviado!',
                text: 'Aguarde aprovação para fazer login.'
            });
            
            // Limpar o state para não mostrar novamente
            window.history.replaceState({}, document.title);
        }
    }, [dispatch, successMessage, navigate]);

    const submitForm = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError('');
        
        try {
            const response = await login({ email, password });
            
            // Mostrar toast de boas-vindas
            const Toast = Swal.mixin({
                toast: true,
                position: 'top-end',
                showConfirmButton: false,
                timer: 3000,
                timerProgressBar: true
            });
            Toast.fire({
                icon: 'success',
                title: `Bem-vindo, ${response.user.name.split(' ')[0]}!`
            });
            
            // Redirecionar para home
            navigate('/');
        } catch (err: any) {
            const errorMessage = err.message || 'Erro ao fazer login. Tente novamente.';
            setError(errorMessage);
            
            // Se for usuário não aprovado, mostrar mensagem especial
            if (errorMessage.toLowerCase().includes('aprovad') || errorMessage.toLowerCase().includes('habilitad')) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Aguardando Aprovação',
                    text: 'Seu cadastro ainda não foi aprovado por um administrador. Por favor, aguarde.',
                    confirmButtonText: 'Entendi',
                    confirmButtonColor: '#006B3F',
                    customClass: {
                        popup: 'rounded-2xl',
                    }
                });
            }
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="relative min-h-screen overflow-hidden">
            {/* Fundo Glassmorphism + Mesh Gradient */}
            <div className="absolute inset-0 bg-gradient-to-br from-primary-light via-[#F0F7FF] to-white">
                {/* Mesh Gradient - Shape 1 (Verde Grande) */}
                <div 
                    className="absolute top-0 right-0 w-[700px] h-[700px] rounded-full opacity-30 blur-[120px]"
                    style={{ background: 'radial-gradient(circle, #006B3F 0%, transparent 70%)' }}
                ></div>
                
                {/* Mesh Gradient - Shape 2 (Amarelo) */}
                <div 
                    className="absolute top-1/4 left-0 w-[600px] h-[600px] rounded-full opacity-25 blur-[100px]"
                    style={{ background: 'radial-gradient(circle, #FFCC00 0%, transparent 70%)' }}
                ></div>
                
                {/* Mesh Gradient - Shape 3 (Verde Pastel) */}
                <div 
                    className="absolute bottom-0 left-1/3 w-[650px] h-[650px] rounded-full opacity-35 blur-[90px]"
                    style={{ background: 'radial-gradient(circle, #C9FBE2 0%, transparent 70%)' }}
                ></div>
                
                {/* Mesh Gradient - Shape 4 (Amarelo Claro) */}
                <div 
                    className="absolute bottom-1/4 right-1/4 w-[500px] h-[500px] rounded-full opacity-20 blur-[100px]"
                    style={{ background: 'radial-gradient(circle, #FFF9E6 0%, transparent 70%)' }}
                ></div>
                
                {/* Linhas curvas decorativas */}
                <svg className="absolute inset-0 w-full h-full opacity-[0.06]" xmlns="http://www.w3.org/2000/svg">
                    <path d="M0,100 Q400,50 800,100 T1600,100" stroke="#006B3F" strokeWidth="2" fill="none" />
                    <path d="M0,300 Q500,250 1000,300 T2000,300" stroke="#FFCC00" strokeWidth="2" fill="none" />
                    <path d="M0,500 Q600,450 1200,500 T2400,500" stroke="#C9FBE2" strokeWidth="1.5" fill="none" />
                </svg>
                
                {/* Pattern de pontos */}
                <div 
                    className="absolute inset-0 opacity-[0.02]"
                    style={{ backgroundImage: 'radial-gradient(circle, #006B3F 1px, transparent 1px)', backgroundSize: '40px 40px' }}
                ></div>
            </div>
            
            <div className="relative flex min-h-screen items-center justify-center px-6 py-10 sm:px-16">
                
                {/* Elementos decorativos flutuantes */}
                <div className="absolute top-20 left-10 w-32 h-32 rounded-full bg-gradient-to-br from-primary/20 to-secondary/20 backdrop-blur-sm border border-white/30 hidden lg:block animate-float"></div>
                <div className="absolute bottom-32 right-20 w-24 h-24 rounded-full bg-gradient-to-br from-secondary/20 to-primary/20 backdrop-blur-sm border border-white/30 hidden lg:block animate-float-delayed"></div>
                <div className="absolute top-1/3 right-10 w-20 h-20 rounded-lg rotate-45 bg-gradient-to-br from-primary/15 to-secondary/15 backdrop-blur-sm border border-white/20 hidden lg:block animate-float-slow"></div>
                
                {/* Container Principal - Glassmorphism Card */}
                <div className="relative flex w-full max-w-[1100px] flex-col justify-between overflow-hidden rounded-2xl bg-white/80 backdrop-blur-xl shadow-[0_8px_32px_rgba(0,0,0,0.08)] border border-white/20 lg:min-h-[600px] lg:flex-row">
                    
                    {/* Lado Esquerdo - Brand Section */}
                    <div className="relative hidden w-full items-center justify-center bg-[#0A3D2A] p-12 lg:flex lg:w-[40%] overflow-hidden">
                        {/* Grid de pontos de fundo */}
                        <div className="absolute inset-0 opacity-[0.05]" style={{
                            backgroundImage: 'radial-gradient(circle, #FFCC00 1px, transparent 1px)',
                            backgroundSize: '24px 24px'
                        }}></div>
                        
                        {/* Formas orgânicas */}
                        <div className="absolute top-0 right-0 w-[600px] h-[600px] bg-gradient-to-br from-primary/20 to-transparent rounded-full blur-3xl"></div>
                        <div className="absolute bottom-0 left-0 w-[400px] h-[400px] bg-gradient-to-tr from-secondary/10 to-transparent rounded-full blur-3xl"></div>
                        
                        <div className="relative z-10 flex flex-col items-center justify-center h-full">
                            {/* Ícone/Logo */}
                            <div className="mb-8">
                                <div className="w-20 h-20 rounded-2xl bg-white/10 backdrop-blur-sm flex items-center justify-center border border-white/20">
                                    <svg className="w-10 h-10 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                                    </svg>
                                </div>
                            </div>
                            
                            {/* Texto */}
                            <div className="text-center">
                                <h2 className="text-white text-2xl font-bold mb-3">Gestão de Funcionários</h2>
                                <p className="text-white/70 text-sm max-w-[280px]">
                                    Sistema completo para gerenciamento de equipes e colaboradores
                                </p>
                            </div>
                            
                            {/* Decoração */}
                            <div className="mt-12 flex gap-2">
                                <div className="w-2 h-2 rounded-full bg-secondary"></div>
                                <div className="w-2 h-2 rounded-full bg-white/40"></div>
                                <div className="w-2 h-2 rounded-full bg-white/40"></div>
                            </div>
                        </div>
                    </div>
                    
                    {/* Lado Direito - Formulário */}
                    <div className="relative flex w-full flex-col items-center justify-center px-6 py-12 sm:px-12 lg:w-[60%]">
                        
                        {/* Logo Mobile */}
                        <div className="flex w-full max-w-[420px] items-center justify-center mb-8 lg:hidden">
                            <div className="w-12 h-12 rounded-xl bg-primary flex items-center justify-center">
                                <svg className="w-6 h-6 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                                </svg>
                            </div>
                        </div>
                        
                        {/* Formulário */}
                        <div className="w-full max-w-[420px]">
                            <div className="mb-8">
                                <h1 className="text-2xl font-bold !leading-tight text-slate-900 mb-2">
                                    Bem-vindo de volta
                                </h1>
                                <p className="text-sm text-slate-500">
                                    Entre com suas credenciais para acessar o sistema
                                </p>
                            </div>

                            {/* Mensagem de sucesso do cadastro */}
                            {successMessage && (
                                <div className="mb-5 rounded-lg border border-green-200 bg-green-50 p-4 flex items-start gap-3">
                                    <svg className="w-5 h-5 text-green-600 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                                    </svg>
                                    <p className="text-sm text-green-800">{successMessage}</p>
                                </div>
                            )}

                            {/* Mensagem de erro */}
                            {error && (
                                <div className="mb-5 rounded-lg border border-red-200 bg-red-50 p-4 flex items-start gap-3">
                                    <svg className="w-5 h-5 text-red-600 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                                    </svg>
                                    <p className="text-sm text-red-800">{error}</p>
                                </div>
                            )}

                            <form className="space-y-4" onSubmit={submitForm}>
                                <div>
                                    <label htmlFor="Email" className="block text-xs font-medium text-slate-700 mb-1.5">
                                        E-mail
                                    </label>
                                    <div className="relative">
                                        <input 
                                            id="Email" 
                                            type="email" 
                                            placeholder="seu@email.com" 
                                            className="form-input rounded-lg ps-10 placeholder:text-slate-400 border-slate-200 focus:border-primary h-11"
                                            value={email}
                                            onChange={(e) => setEmail(e.target.value)}
                                            required
                                            disabled={loading}
                                        />
                                        <span className="absolute start-3 top-1/2 -translate-y-1/2 text-slate-400">
                                            <IconMail className="w-5 h-5" fill={true} />
                                        </span>
                                    </div>
                                </div>
                                
                                <div>
                                    <label htmlFor="Password" className="block text-xs font-medium text-slate-700 mb-1.5">
                                        Senha
                                    </label>
                                    <div className="relative">
                                        <input 
                                            id="Password" 
                                            type="password" 
                                            placeholder="••••••••" 
                                            className="form-input rounded-lg ps-10 placeholder:text-slate-400 border-slate-200 focus:border-primary h-11"
                                            value={password}
                                            onChange={(e) => setPassword(e.target.value)}
                                            required
                                            disabled={loading}
                                        />
                                        <span className="absolute start-3 top-1/2 -translate-y-1/2 text-slate-400">
                                            <IconLockDots className="w-5 h-5" fill={true} />
                                        </span>
                                    </div>
                                </div>
                                
                                <div className="flex items-center justify-between pt-1">
                                    <label className="flex cursor-pointer items-center gap-2">
                                        <input 
                                            type="checkbox" 
                                            className="form-checkbox w-4 h-4 rounded border-slate-300 text-primary focus:ring-primary"
                                            checked={rememberMe}
                                            onChange={(e) => setRememberMe(e.target.checked)}
                                        />
                                        <span className="text-sm text-slate-600">Lembrar-me</span>
                                    </label>
                                    <Link to="/auth/password-reset" className="text-sm text-primary hover:text-primary/80 font-medium">
                                        Esqueceu a senha?
                                    </Link>
                                </div>
                                
                                <button 
                                    type="submit" 
                                    className="!mt-6 w-full h-11 rounded-lg bg-primary hover:bg-primary/90 text-white font-medium shadow-sm transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
                                    disabled={loading}
                                >
                                    {loading ? (
                                        <span className="flex items-center justify-center gap-2">
                                            <svg className="animate-spin h-5 w-5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                            </svg>
                                            <span>Entrando...</span>
                                        </span>
                                    ) : (
                                        'Entrar'
                                    )}
                                </button>
                            </form>

                            <div className="text-center mt-6">
                                <p className="text-sm text-slate-600">
                                    Não tem uma conta?{' '}
                                    <Link to="/auth/boxed-signup" className="text-primary hover:text-primary/80 font-semibold">
                                        Cadastre-se
                                    </Link>
                                </p>
                            </div>
                        </div>
                        
                        <p className="absolute bottom-6 w-full text-center text-xs text-slate-400">
                            © {new Date().getFullYear()} - Gestão de Funcionários
                        </p>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default LoginBoxed;
