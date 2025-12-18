import { useEffect, useState } from 'react';
import { useDispatch } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { setPageTitle } from '../../store/themeConfigSlice';
import { getRoles, deleteRole, getCurrentUser, RoleDto } from '../../services/api';
import Swal from 'sweetalert2';
import Breadcrumb from '../../components/Breadcrumb';
import IconPencil from '../../components/Icon/IconPencil';
import IconTrash from '../../components/Icon/IconTrash';
import IconPlus from '../../components/Icon/IconPlus';
import RoleModal from './RoleModal';

const RoleList = () => {
    const dispatch = useDispatch();
    const navigate = useNavigate();
    const currentUser = getCurrentUser();
    
    const [roles, setRoles] = useState<RoleDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [modalOpen, setModalOpen] = useState(false);
    const [editingRole, setEditingRole] = useState<RoleDto | null>(null);

    useEffect(() => {
        dispatch(setPageTitle('Gestão de Cargos'));
        
        // Verificar permissão para acessar a página
        if (!currentUser?.canManageRoles) {
            Swal.fire({
                icon: 'warning',
                title: 'Sem Permissão',
                text: 'Você não tem permissão para gerenciar cargos.',
                confirmButtonColor: '#006B3F',
            }).then(() => navigate('/'));
            return;
        }
        
        loadRoles();
    }, [dispatch, navigate, currentUser]);

    const loadRoles = async () => {
        try {
            setLoading(true);
            const data = await getRoles();
            setRoles(data);
        } catch (error: any) {
            Swal.fire({
                icon: 'error',
                title: 'Erro',
                text: error.message || 'Erro ao carregar cargos',
                confirmButtonColor: '#006B3F',
            });
        } finally {
            setLoading(false);
        }
    };

    const handleCreate = () => {
        setEditingRole(null);
        setModalOpen(true);
    };

    const handleEdit = (role: RoleDto) => {
        setEditingRole(role);
        setModalOpen(true);
    };

    const handleDelete = async (role: RoleDto) => {
        // Verificar se há funcionários vinculados (isso é validação de negócio, não de permissão)
        if (role.employeeCount > 0) {
            Swal.fire({
                icon: 'warning',
                title: 'Cargo em uso',
                text: `Este cargo possui ${role.employeeCount} funcionário(s) vinculado(s). Remova-os primeiro.`,
                confirmButtonColor: '#006B3F',
            });
            return;
        }

        const result = await Swal.fire({
            icon: 'warning',
            title: 'Confirmar Exclusão',
            html: `Deseja realmente excluir o cargo <strong>${role.name}</strong>?`,
            showCancelButton: true,
            confirmButtonText: 'Sim, excluir',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#dc2626',
            cancelButtonColor: '#6b7280',
        });

        if (result.isConfirmed) {
            try {
                await deleteRole(role.id);
                Swal.fire({
                    icon: 'success',
                    title: 'Excluído!',
                    text: 'Cargo excluído com sucesso.',
                    confirmButtonColor: '#006B3F',
                });
                loadRoles();
            } catch (error: any) {
                Swal.fire({
                    icon: 'error',
                    title: 'Erro',
                    text: error.message || 'Erro ao excluir cargo',
                    confirmButtonColor: '#006B3F',
                });
            }
        }
    };

    const handleModalClose = (saved: boolean) => {
        setModalOpen(false);
        setEditingRole(null);
        if (saved) {
            loadRoles();
        }
    };

    const getPermissionBadges = (role: RoleDto) => {
        const badges = [];
        if (role.canApproveRegistrations) badges.push('Aprovar');
        if (role.canCreateEmployees) badges.push('Criar');
        if (role.canEditEmployees) badges.push('Editar');
        if (role.canDeleteEmployees) badges.push('Excluir');
        if (role.canManageRoles) badges.push('Cargos');
        return badges;
    };

    return (
        <div>
            <Breadcrumb 
                items={[
                    { label: 'Configurações', path: '/settings/roles' },
                    { label: 'Gestão de Cargos' }
                ]} 
            />

            <div className="panel">
                <div className="flex flex-col gap-5 md:flex-row md:items-center md:justify-between mb-5">
                    <div>
                        <h5 className="text-lg font-semibold text-slate-900">Gestão de Cargos</h5>
                        <p className="text-sm text-slate-500">Gerencie os cargos e permissões do sistema</p>
                    </div>
                    {currentUser?.canManageRoles && (
                        <button
                            onClick={handleCreate}
                            className="btn btn-primary flex items-center gap-2"
                        >
                            <IconPlus className="w-5 h-5" />
                            <span>Novo Cargo</span>
                        </button>
                    )}
                </div>

            {loading ? (
                <div className="flex items-center justify-center py-20">
                    <div className="animate-spin rounded-full h-10 w-10 border-4 border-primary border-t-transparent"></div>
                </div>
            ) : roles.length === 0 ? (
                <div className="flex flex-col items-center justify-center py-20 text-slate-500">
                    <svg className="w-16 h-16 mb-4 text-slate-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                    </svg>
                    <p className="text-lg font-medium">Nenhum cargo encontrado</p>
                    <p className="text-sm">Cadastre o primeiro cargo</p>
                </div>
            ) : (
                <div className="table-responsive">
                    <table className="table-striped">
                        <thead>
                            <tr>
                                <th>Nome</th>
                                <th>Descrição</th>
                                <th className="text-center">Nível</th>
                                <th>Permissões</th>
                                <th className="text-center">Funcionários</th>
                                {currentUser?.canManageRoles && <th className="text-center">Ações</th>}
                            </tr>
                        </thead>
                        <tbody>
                            {roles.map((role) => (
                                <tr key={role.id}>
                                    <td>
                                        <div className="flex items-center gap-3">
                                            <div className={`w-10 h-10 rounded-full flex items-center justify-center font-semibold ${
                                                role.hierarchyLevel >= 100 
                                                    ? 'bg-purple-100 text-purple-700' 
                                                    : role.hierarchyLevel >= 50 
                                                        ? 'bg-blue-100 text-blue-700' 
                                                        : 'bg-slate-100 text-slate-700'
                                            }`}>
                                                {role.name.charAt(0).toUpperCase()}
                                            </div>
                                            <span className="font-medium text-slate-900">{role.name}</span>
                                        </div>
                                    </td>
                                    <td className="text-slate-600 max-w-xs truncate">{role.description || '-'}</td>
                                    <td className="text-center">
                                        <span className={`inline-flex items-center justify-center w-10 h-10 rounded-full text-sm font-bold ${
                                            role.hierarchyLevel >= 100 
                                                ? 'bg-purple-100 text-purple-700' 
                                                : role.hierarchyLevel >= 50 
                                                    ? 'bg-blue-100 text-blue-700' 
                                                    : 'bg-slate-100 text-slate-700'
                                        }`}>
                                            {role.hierarchyLevel}
                                        </span>
                                    </td>
                                    <td>
                                        <div className="flex flex-wrap gap-1">
                                            {getPermissionBadges(role).map((badge, idx) => (
                                                <span key={idx} className="badge bg-primary/10 text-primary text-xs">
                                                    {badge}
                                                </span>
                                            ))}
                                            {getPermissionBadges(role).length === 0 && (
                                                <span className="text-slate-400 text-sm">Nenhuma</span>
                                            )}
                                        </div>
                                    </td>
                                    <td className="text-center">
                                        <span className={`badge ${role.employeeCount > 0 ? 'bg-green-100 text-green-700' : 'bg-slate-100 text-slate-500'}`}>
                                            {role.employeeCount}
                                        </span>
                                    </td>
                                    {currentUser?.canManageRoles && (
                                        <td>
                                            <div className="flex items-center justify-center gap-2">
                                                <button
                                                    type="button"
                                                    className="btn btn-sm btn-outline-dark"
                                                    onClick={() => handleEdit(role)}
                                                    title="Editar"
                                                >
                                                    <IconPencil className="w-4 h-4" />
                                                </button>
                                                <button
                                                    type="button"
                                                    className="btn btn-sm btn-outline-dark"
                                                    onClick={() => handleDelete(role)}
                                                    title="Excluir"
                                                    disabled={role.employeeCount > 0}
                                                >
                                                    <IconTrash className="w-4 h-4" />
                                                </button>
                                            </div>
                                        </td>
                                    )}
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            {/* Modal de criação/edição */}
            <RoleModal
                isOpen={modalOpen}
                onClose={handleModalClose}
                role={editingRole}
            />
            </div>
        </div>
    );
};

export default RoleList;

