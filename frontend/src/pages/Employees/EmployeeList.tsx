import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useDispatch } from 'react-redux';
import { setPageTitle } from '../../store/themeConfigSlice';
import { getEmployees, deleteEmployee, getCurrentUser, EmployeeDto } from '../../services/api';
import Swal from 'sweetalert2';
import Breadcrumb from '../../components/Breadcrumb';
import IconPencil from '../../components/Icon/IconPencil';
import IconTrash from '../../components/Icon/IconTrash';
import IconPlus from '../../components/Icon/IconPlus';
import IconSearch from '../../components/Icon/IconSearch';

const EmployeeList = () => {
    const dispatch = useDispatch();
    const navigate = useNavigate();
    const currentUser = getCurrentUser();
    
    const [employees, setEmployees] = useState<EmployeeDto[]>([]);
    const [filteredEmployees, setFilteredEmployees] = useState<EmployeeDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [search, setSearch] = useState('');

    useEffect(() => {
        dispatch(setPageTitle('Funcionários'));
        loadEmployees();
    }, [dispatch]);

    useEffect(() => {
        // Filtrar funcionários pelo termo de busca
        if (search.trim() === '') {
            setFilteredEmployees(employees);
        } else {
            const term = search.toLowerCase();
            setFilteredEmployees(
                employees.filter(
                    (emp) =>
                        emp.name.toLowerCase().includes(term) ||
                        emp.email.toLowerCase().includes(term) ||
                        emp.documentNumber.includes(term) ||
                        emp.role.name.toLowerCase().includes(term)
                )
            );
        }
    }, [search, employees]);

    const loadEmployees = async () => {
        try {
            setLoading(true);
            const data = await getEmployees();
            setEmployees(data);
            setFilteredEmployees(data);
        } catch (error: any) {
            Swal.fire({
                icon: 'error',
                title: 'Erro',
                text: error.message || 'Erro ao carregar funcionários',
                confirmButtonColor: '#006B3F',
            });
        } finally {
            setLoading(false);
        }
    };

    const handleDelete = async (employee: EmployeeDto) => {
        if (!currentUser?.canDeleteEmployees) {
            Swal.fire({
                icon: 'warning',
                title: 'Sem Permissão',
                text: 'Você não tem permissão para excluir funcionários.',
                confirmButtonColor: '#006B3F',
            });
            return;
        }

        if (employee.id === currentUser?.id) {
            Swal.fire({
                icon: 'warning',
                title: 'Ação não permitida',
                text: 'Você não pode excluir seu próprio cadastro.',
                confirmButtonColor: '#006B3F',
            });
            return;
        }

        const result = await Swal.fire({
            icon: 'warning',
            title: 'Confirmar Exclusão',
            html: `Deseja realmente excluir o funcionário <strong>${employee.name}</strong>?<br><br>Esta ação não pode ser desfeita.`,
            showCancelButton: true,
            confirmButtonText: 'Sim, excluir',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#dc2626',
            cancelButtonColor: '#6b7280',
        });

        if (result.isConfirmed) {
            try {
                await deleteEmployee(employee.id);
                Swal.fire({
                    icon: 'success',
                    title: 'Excluído!',
                    text: 'Funcionário excluído com sucesso.',
                    confirmButtonColor: '#006B3F',
                });
                loadEmployees();
            } catch (error: any) {
                Swal.fire({
                    icon: 'error',
                    title: 'Erro',
                    text: error.message || 'Erro ao excluir funcionário',
                    confirmButtonColor: '#006B3F',
                });
            }
        }
    };

    const handleEdit = (employee: EmployeeDto) => {
        if (!currentUser?.canEditEmployees) {
            Swal.fire({
                icon: 'warning',
                title: 'Sem Permissão',
                text: 'Você não tem permissão para editar funcionários.',
                confirmButtonColor: '#006B3F',
            });
            return;
        }

        if (employee.id === currentUser?.id) {
            Swal.fire({
                icon: 'warning',
                title: 'Ação não permitida',
                text: 'Você não pode editar seu próprio cadastro. Use "Meu Perfil".',
                confirmButtonColor: '#006B3F',
            });
            return;
        }

        navigate(`/employees/edit/${employee.id}`);
    };

    const formatDate = (dateString: string) => {
        return new Date(dateString).toLocaleDateString('pt-BR');
    };

    const formatCPF = (cpf: string) => {
        const numbers = cpf.replace(/\D/g, '');
        return numbers.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
    };

    const getPhotoUrl = (photoUrl: string | null) => {
        if (photoUrl) {
            return `http://localhost:5000${photoUrl}`;
        }
        return null;
    };

    return (
        <div>
            <Breadcrumb 
                items={[
                    { label: 'Gestão de Usuários', path: '/employees' },
                    { label: 'Funcionários' }
                ]} 
            />

            <div className="panel">
                <div className="flex flex-col gap-5 md:flex-row md:items-center md:justify-between mb-5">
                    <h5 className="text-lg font-semibold text-slate-900">Lista de Funcionários</h5>
                    <div className="flex flex-col gap-3 sm:flex-row">
                        <div className="relative">
                            <input
                                type="text"
                                className="form-input pl-10 w-full sm:w-72"
                                placeholder="Buscar por nome, email, CPF..."
                                value={search}
                                onChange={(e) => setSearch(e.target.value)}
                            />
                            <span className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400">
                                <IconSearch className="w-5 h-5" />
                            </span>
                        </div>
                        {currentUser?.canCreateEmployees && (
                            <Link
                                to="/employees/create"
                                className="btn btn-primary flex items-center gap-2"
                            >
                                <IconPlus className="w-5 h-5" />
                                <span>Novo Funcionário</span>
                            </Link>
                        )}
                    </div>
                </div>

                {loading ? (
                    <div className="flex items-center justify-center py-20">
                        <div className="animate-spin rounded-full h-10 w-10 border-4 border-primary border-t-transparent"></div>
                    </div>
                ) : filteredEmployees.length === 0 ? (
                    <div className="flex flex-col items-center justify-center py-20 text-slate-500">
                        <svg className="w-16 h-16 mb-4 text-slate-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" />
                        </svg>
                        <p className="text-lg font-medium">Nenhum funcionário encontrado</p>
                        <p className="text-sm">
                            {search ? 'Tente outro termo de busca' : 'Cadastre o primeiro funcionário'}
                        </p>
                    </div>
                ) : (
                    <div className="table-responsive">
                        <table className="table-striped">
                            <thead>
                                <tr>
                                    <th>Funcionário</th>
                                    <th>E-mail</th>
                                    <th>CPF</th>
                                    <th>Cargo</th>
                                    <th>Gerente</th>
                                    <th>Status</th>
                                    <th className="text-center">Ações</th>
                                </tr>
                            </thead>
                            <tbody>
                                {filteredEmployees.map((employee) => (
                                    <tr key={employee.id}>
                                        <td>
                                            <div className="flex items-center gap-3">
                                                {getPhotoUrl(employee.photoUrl) ? (
                                                    <img 
                                                        src={getPhotoUrl(employee.photoUrl)!} 
                                                        alt={employee.name}
                                                        className="w-10 h-10 rounded-full object-cover border border-slate-200"
                                                    />
                                                ) : (
                                                    <div className="w-10 h-10 rounded-full bg-primary/10 flex items-center justify-center text-primary font-semibold">
                                                        {employee.name.charAt(0).toUpperCase()}
                                                    </div>
                                                )}
                                                <div>
                                                    <p className="font-medium text-slate-900">{employee.name}</p>
                                                    <p className="text-xs text-slate-500">
                                                        {employee.age} anos • Desde {formatDate(employee.createdAt)}
                                                    </p>
                                                </div>
                                            </div>
                                        </td>
                                        <td>{employee.email}</td>
                                        <td className="font-mono text-sm">{formatCPF(employee.documentNumber)}</td>
                                        <td>
                                            <span className={`badge ${
                                                employee.role.hierarchyLevel >= 100 
                                                    ? 'bg-purple-100 text-purple-700' 
                                                    : employee.role.hierarchyLevel >= 50 
                                                        ? 'bg-blue-100 text-blue-700' 
                                                        : 'bg-slate-100 text-slate-700'
                                            }`}>
                                                {employee.role.name}
                                            </span>
                                        </td>
                                        <td>{employee.managerName || '-'}</td>
                                        <td>
                                            {employee.enabled ? (
                                                <span className="badge bg-green-100 text-green-700">Ativo</span>
                                            ) : (
                                                <span className="badge bg-amber-100 text-amber-700">Pendente</span>
                                            )}
                                        </td>
                                        <td>
                                            <div className="flex items-center justify-center gap-2">
                                                <button
                                                    type="button"
                                                    className="btn btn-sm btn-outline-dark"
                                                    onClick={() => handleEdit(employee)}
                                                    title="Editar"
                                                >
                                                    <IconPencil className="w-4 h-4" />
                                                </button>
                                                <button
                                                    type="button"
                                                    className="btn btn-sm btn-outline-dark"
                                                    onClick={() => handleDelete(employee)}
                                                    title="Excluir"
                                                >
                                                    <IconTrash className="w-4 h-4" />
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                )}
                
                {!loading && filteredEmployees.length > 0 && (
                    <div className="mt-4 text-sm text-slate-500">
                        Exibindo {filteredEmployees.length} de {employees.length} funcionários
                    </div>
                )}
            </div>
        </div>
    );
};

export default EmployeeList;
