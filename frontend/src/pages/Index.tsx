import { useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useDispatch } from 'react-redux';
import { setPageTitle } from '../store/themeConfigSlice';
import { getCurrentUser } from '../services/api';
import Breadcrumb from '../components/Breadcrumb';
import IconMenuUsers from '../components/Icon/Menu/IconMenuUsers';
import IconSettings from '../components/Icon/IconSettings';

const Index = () => {
    const dispatch = useDispatch();
    const currentUser = getCurrentUser();

    useEffect(() => {
        dispatch(setPageTitle('Dashboard'));
    }, [dispatch]);

    return (
        <div>
            <Breadcrumb 
                items={[
                    { label: 'Dashboard' }
                ]} 
            />

            {/* Boas-vindas */}
            <div className="panel mb-5">
                <div className="flex items-center justify-between">
                    <div>
                        <h1 className="text-2xl font-bold text-slate-900">
                            Olá, {currentUser?.name.split(' ')[0]}! 👋
                        </h1>
                        <p className="text-slate-500 mt-1">
                            Bem-vindo ao Sistema de Gestão de Funcionários
                        </p>
                    </div>
                    <div className="hidden md:block">
                        <span className="badge bg-primary/10 text-primary text-sm py-2 px-4">
                            {currentUser?.role.name}
                        </span>
                    </div>
                </div>
            </div>

            {/* Cards de Acesso Rápido */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
                {/* Funcionários */}
                <Link to="/employees" className="panel hover:shadow-lg transition-shadow group">
                    <div className="flex items-center gap-4">
                        <div className="w-14 h-14 rounded-xl bg-primary/10 flex items-center justify-center group-hover:bg-primary/20 transition-colors">
                            <IconMenuUsers className="w-7 h-7 text-primary" />
                        </div>
                        <div>
                            <h3 className="text-lg font-semibold text-slate-900">Funcionários</h3>
                            <p className="text-sm text-slate-500">Gerenciar funcionários</p>
                        </div>
                    </div>
                </Link>

                {/* Cargos */}
                {currentUser?.canManageRoles && (
                    <Link to="/roles" className="panel hover:shadow-lg transition-shadow group">
                        <div className="flex items-center gap-4">
                            <div className="w-14 h-14 rounded-xl bg-blue-50 flex items-center justify-center group-hover:bg-blue-100 transition-colors">
                                <IconSettings className="w-7 h-7 text-blue-600" />
                            </div>
                            <div>
                                <h3 className="text-lg font-semibold text-slate-900">Gestão de Cargos</h3>
                                <p className="text-sm text-slate-500">Configurar cargos e permissões</p>
                            </div>
                        </div>
                    </Link>
                )}
            </div>

            {/* Informações do Usuário */}
            <div className="panel mt-5">
                <h5 className="text-lg font-semibold text-slate-900 mb-4">Suas Permissões</h5>
                <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4">
                    <div className={`p-4 rounded-lg text-center ${currentUser?.canApproveRegistrations ? 'bg-green-50 text-green-700' : 'bg-slate-50 text-slate-400'}`}>
                        <p className="text-sm font-medium">Aprovar Cadastros</p>
                        <p className="text-xs mt-1">{currentUser?.canApproveRegistrations ? '✓ Sim' : '✗ Não'}</p>
                    </div>
                    <div className={`p-4 rounded-lg text-center ${currentUser?.canCreateEmployees ? 'bg-green-50 text-green-700' : 'bg-slate-50 text-slate-400'}`}>
                        <p className="text-sm font-medium">Criar Funcionários</p>
                        <p className="text-xs mt-1">{currentUser?.canCreateEmployees ? '✓ Sim' : '✗ Não'}</p>
                    </div>
                    <div className={`p-4 rounded-lg text-center ${currentUser?.canEditEmployees ? 'bg-green-50 text-green-700' : 'bg-slate-50 text-slate-400'}`}>
                        <p className="text-sm font-medium">Editar Funcionários</p>
                        <p className="text-xs mt-1">{currentUser?.canEditEmployees ? '✓ Sim' : '✗ Não'}</p>
                    </div>
                    <div className={`p-4 rounded-lg text-center ${currentUser?.canDeleteEmployees ? 'bg-green-50 text-green-700' : 'bg-slate-50 text-slate-400'}`}>
                        <p className="text-sm font-medium">Excluir Funcionários</p>
                        <p className="text-xs mt-1">{currentUser?.canDeleteEmployees ? '✓ Sim' : '✗ Não'}</p>
                    </div>
                    <div className={`p-4 rounded-lg text-center ${currentUser?.canManageRoles ? 'bg-green-50 text-green-700' : 'bg-slate-50 text-slate-400'}`}>
                        <p className="text-sm font-medium">Gerenciar Cargos</p>
                        <p className="text-xs mt-1">{currentUser?.canManageRoles ? '✓ Sim' : '✗ Não'}</p>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Index;
