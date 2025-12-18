import { useEffect, useState, useRef } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Link, useNavigate } from 'react-router-dom';
import { logout, getCurrentUser, getEmployeeById } from '../../services/api';
import { IRootState } from '../../store';
import { toggleSidebar } from '../../store/themeConfigSlice';
import Dropdown from '../Dropdown';
import IconMenu from '../Icon/IconMenu';
import IconUser from '../Icon/IconUser';
import IconLogout from '../Icon/IconLogout';

const Header = () => {
    const navigate = useNavigate();
    const dispatch = useDispatch();
    const themeConfig = useSelector((state: IRootState) => state.themeConfig);
    const isRtl = themeConfig.rtlClass === 'rtl';
    
    const currentUser = getCurrentUser();
    const [userPhoto, setUserPhoto] = useState<string | null>(null);
    const photoLoadedRef = useRef(false);

    useEffect(() => {
        // Carregar foto do usuário logado apenas uma vez
        if (currentUser?.id && !photoLoadedRef.current) {
            photoLoadedRef.current = true;
            const loadUserPhoto = async () => {
                try {
                    const employee = await getEmployeeById(currentUser.id);
                    if (employee.photoUrl) {
                        setUserPhoto(`http://localhost:5000${employee.photoUrl}`);
                    }
                } catch (error) {
                    console.error('Erro ao carregar foto do usuário:', error);
                }
            };
            loadUserPhoto();
        }
    }, [currentUser?.id]);

    const handleLogout = () => {
        logout();
        navigate('/auth/login');
    };

    return (
        <header className={`z-40 ${themeConfig.semidark && themeConfig.menu === 'horizontal' ? 'dark' : ''}`}>
            <div className="shadow-sm">
                <div className="relative bg-white flex w-full items-center px-5 py-2.5 dark:bg-black">
                    {/* Logo Mobile + Menu Toggle */}
                    <div className="horizontal-logo flex lg:hidden justify-between items-center ltr:mr-2 rtl:ml-2">
                        <Link to="/" className="main-logo flex items-center shrink-0">
                            <img className="w-8 ltr:-ml-1 rtl:-mr-1 inline" src="/assets/images/logo.svg" alt="logo" />
                            <span className="text-xl ltr:ml-1.5 rtl:mr-1.5 font-semibold align-middle hidden md:inline dark:text-white-light transition-all duration-300">
                                Gestão RH
                            </span>
                        </Link>
                        <button
                            type="button"
                            className="collapse-icon flex-none dark:text-[#d0d2d6] hover:text-primary dark:hover:text-primary flex lg:hidden ltr:ml-2 rtl:mr-2 p-2 rounded-full bg-white-light/40 dark:bg-dark/40 hover:bg-white-light/90 dark:hover:bg-dark/60"
                            onClick={() => dispatch(toggleSidebar())}
                        >
                            <IconMenu className="w-5 h-5" />
                        </button>
                    </div>

                    {/* Spacer */}
                    <div className="flex-1"></div>

                    {/* User Dropdown */}
                    <div className="dropdown shrink-0 flex">
                        <Dropdown
                            offset={[0, 8]}
                            placement={`${isRtl ? 'bottom-start' : 'bottom-end'}`}
                            btnClassName="relative group block"
                            button={
                                userPhoto ? (
                                    <img 
                                        className="w-10 h-10 rounded-full object-cover border-2 border-slate-200 group-hover:border-primary transition-colors" 
                                        src={userPhoto} 
                                        alt="Foto do usuário" 
                                    />
                                ) : (
                                    <div className="w-10 h-10 rounded-full bg-primary flex items-center justify-center text-white font-bold text-lg group-hover:bg-primary/90 transition-colors">
                                        {currentUser?.name?.charAt(0).toUpperCase() || 'U'}
                                    </div>
                                )
                            }
                        >
                            <ul className="text-dark dark:text-white-dark !py-0 w-[250px] font-semibold dark:text-white-light/90">
                                <li>
                                    <div className="flex items-center px-4 py-4 border-b border-slate-100 dark:border-slate-700">
                                        {userPhoto ? (
                                            <img 
                                                className="w-12 h-12 rounded-full object-cover border-2 border-slate-200" 
                                                src={userPhoto} 
                                                alt="Foto do usuário" 
                                            />
                                        ) : (
                                            <div className="w-12 h-12 rounded-full bg-primary flex items-center justify-center text-white font-bold text-xl">
                                                {currentUser?.name?.charAt(0).toUpperCase() || 'U'}
                                            </div>
                                        )}
                                        <div className="ltr:pl-4 rtl:pr-4 truncate">
                                            <h4 className="text-base font-semibold text-slate-900 dark:text-white">
                                                {currentUser?.name || 'Usuário'}
                                            </h4>
                                            <p className="text-xs text-slate-500 dark:text-slate-400">
                                                {currentUser?.role?.name || 'Cargo'}
                                            </p>
                                        </div>
                                    </div>
                                </li>
                                <li>
                                    <Link to="/profile" className="flex items-center px-4 py-3 hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors">
                                        <IconUser className="w-5 h-5 ltr:mr-3 rtl:ml-3 text-slate-500" />
                                        <span>Meu Perfil</span>
                                    </Link>
                                </li>
                                <li className="border-t border-slate-100 dark:border-slate-700">
                                    <button 
                                        onClick={handleLogout} 
                                        className="flex items-center px-4 py-3 w-full text-danger hover:bg-danger/10 transition-colors"
                                    >
                                        <IconLogout className="w-5 h-5 ltr:mr-3 rtl:ml-3 rotate-90" />
                                        <span>Sair</span>
                                    </button>
                                </li>
                            </ul>
                        </Dropdown>
                    </div>
                </div>
            </div>
        </header>
    );
};

export default Header;
