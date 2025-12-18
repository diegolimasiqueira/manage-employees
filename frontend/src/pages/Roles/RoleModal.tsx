import { useEffect, useState, Fragment } from 'react';
import { Dialog, Transition } from '@headlessui/react';
import { createRole, updateRole, getCurrentUser, RoleDto, CreateRoleRequest, UpdateRoleRequest } from '../../services/api';
import Swal from 'sweetalert2';
import IconX from '../../components/Icon/IconX';

interface RoleModalProps {
    isOpen: boolean;
    onClose: (saved: boolean) => void;
    role: RoleDto | null;
}

interface FormErrors {
    name?: string;
    description?: string;
    hierarchyLevel?: string;
}

const RoleModal = ({ isOpen, onClose, role }: RoleModalProps) => {
    const currentUser = getCurrentUser();
    const isEditing = !!role;
    
    const [saving, setSaving] = useState(false);
    const [errors, setErrors] = useState<FormErrors>({});
    
    // Form fields
    const [name, setName] = useState('');
    const [description, setDescription] = useState('');
    const [hierarchyLevel, setHierarchyLevel] = useState(10);
    const [canApproveRegistrations, setCanApproveRegistrations] = useState(false);
    const [canCreateEmployees, setCanCreateEmployees] = useState(false);
    const [canEditEmployees, setCanEditEmployees] = useState(false);
    const [canDeleteEmployees, setCanDeleteEmployees] = useState(false);
    const [canManageRoles, setCanManageRoles] = useState(false);

    useEffect(() => {
        if (isOpen) {
            if (role) {
                // Edição - preencher com dados do cargo
                setName(role.name);
                setDescription(role.description || '');
                setHierarchyLevel(role.hierarchyLevel);
                setCanApproveRegistrations(role.canApproveRegistrations);
                setCanCreateEmployees(role.canCreateEmployees);
                setCanEditEmployees(role.canEditEmployees);
                setCanDeleteEmployees(role.canDeleteEmployees);
                setCanManageRoles(role.canManageRoles);
            } else {
                // Criação - limpar campos
                setName('');
                setDescription('');
                setHierarchyLevel(10);
                setCanApproveRegistrations(false);
                setCanCreateEmployees(false);
                setCanEditEmployees(false);
                setCanDeleteEmployees(false);
                setCanManageRoles(false);
            }
            setErrors({});
        }
    }, [isOpen, role]);

    const validate = (): boolean => {
        const newErrors: FormErrors = {};

        if (!name.trim()) {
            newErrors.name = 'Nome é obrigatório';
        } else if (name.trim().length < 3) {
            newErrors.name = 'Nome deve ter pelo menos 3 caracteres';
        }

        if (hierarchyLevel < 1 || hierarchyLevel > 100) {
            newErrors.hierarchyLevel = 'Nível deve estar entre 1 e 100';
        }

        // Não pode criar cargo com nível igual ou superior ao seu
        const userLevel = currentUser?.role.hierarchyLevel || 0;
        if (hierarchyLevel >= userLevel) {
            newErrors.hierarchyLevel = `Nível deve ser menor que ${userLevel} (seu nível)`;
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
            const data: CreateRoleRequest | UpdateRoleRequest = {
                name: name.trim(),
                description: description.trim(),
                hierarchyLevel,
                canApproveRegistrations,
                canCreateEmployees,
                canEditEmployees,
                canDeleteEmployees,
                canManageRoles,
            };

            if (isEditing) {
                await updateRole(role.id, data);
                Swal.fire({
                    icon: 'success',
                    title: 'Sucesso!',
                    text: 'Cargo atualizado com sucesso.',
                    confirmButtonColor: '#006B3F',
                    timer: 2000,
                });
            } else {
                await createRole(data);
                Swal.fire({
                    icon: 'success',
                    title: 'Sucesso!',
                    text: 'Cargo criado com sucesso.',
                    confirmButtonColor: '#006B3F',
                    timer: 2000,
                });
            }

            onClose(true);
        } catch (error: any) {
            Swal.fire({
                icon: 'error',
                title: 'Erro ao salvar',
                text: error.message || 'Erro ao salvar cargo',
                confirmButtonColor: '#006B3F',
            });
        } finally {
            setSaving(false);
        }
    };

    return (
        <Transition appear show={isOpen} as={Fragment}>
            <Dialog as="div" open={isOpen} onClose={() => onClose(false)} className="relative z-50">
                <Transition.Child
                    as={Fragment}
                    enter="ease-out duration-300"
                    enterFrom="opacity-0"
                    enterTo="opacity-100"
                    leave="ease-in duration-200"
                    leaveFrom="opacity-100"
                    leaveTo="opacity-0"
                >
                    <div className="fixed inset-0 bg-black/60" />
                </Transition.Child>

                <div className="fixed inset-0 overflow-y-auto">
                    <div className="flex min-h-full items-center justify-center p-4">
                        <Transition.Child
                            as={Fragment}
                            enter="ease-out duration-300"
                            enterFrom="opacity-0 scale-95"
                            enterTo="opacity-100 scale-100"
                            leave="ease-in duration-200"
                            leaveFrom="opacity-100 scale-100"
                            leaveTo="opacity-0 scale-95"
                        >
                            <Dialog.Panel className="panel w-full max-w-lg overflow-hidden rounded-lg border-0 p-0">
                                {/* Header */}
                                <div className="flex items-center justify-between bg-slate-50 px-5 py-3 dark:bg-slate-800">
                                    <h5 className="text-lg font-bold text-slate-900 dark:text-white">
                                        {isEditing ? 'Editar Cargo' : 'Novo Cargo'}
                                    </h5>
                                    <button
                                        type="button"
                                        className="text-slate-400 hover:text-slate-600"
                                        onClick={() => onClose(false)}
                                    >
                                        <IconX className="w-5 h-5" />
                                    </button>
                                </div>

                                {/* Body */}
                                <form onSubmit={handleSubmit} className="p-5 space-y-4" noValidate>
                                    {/* Nome */}
                                    <div>
                                        <label htmlFor="name" className="block text-sm font-medium text-slate-700 mb-1">
                                            Nome do Cargo <span className="text-red-500">*</span>
                                        </label>
                                        <input
                                            id="name"
                                            type="text"
                                            className={`form-input ${errors.name ? 'border-red-500' : ''}`}
                                            placeholder="Ex: Gerente, Coordenador..."
                                            value={name}
                                            onChange={(e) => setName(e.target.value)}
                                        />
                                        {errors.name && <p className="text-red-500 text-xs mt-1">{errors.name}</p>}
                                    </div>

                                    {/* Descrição */}
                                    <div>
                                        <label htmlFor="description" className="block text-sm font-medium text-slate-700 mb-1">
                                            Descrição
                                        </label>
                                        <textarea
                                            id="description"
                                            className="form-textarea"
                                            placeholder="Descrição das responsabilidades..."
                                            rows={2}
                                            value={description}
                                            onChange={(e) => setDescription(e.target.value)}
                                        />
                                    </div>

                                    {/* Nível Hierárquico */}
                                    <div>
                                        <label htmlFor="hierarchyLevel" className="block text-sm font-medium text-slate-700 mb-1">
                                            Nível Hierárquico <span className="text-red-500">*</span>
                                        </label>
                                        <input
                                            id="hierarchyLevel"
                                            type="number"
                                            min="1"
                                            max={(currentUser?.role.hierarchyLevel || 100) - 1}
                                            className={`form-input ${errors.hierarchyLevel ? 'border-red-500' : ''}`}
                                            value={hierarchyLevel}
                                            onChange={(e) => setHierarchyLevel(parseInt(e.target.value) || 1)}
                                        />
                                        <p className="text-xs text-slate-500 mt-1">
                                            Quanto maior o número, maior a hierarquia. Máximo permitido: {(currentUser?.role.hierarchyLevel || 100) - 1}
                                        </p>
                                        {errors.hierarchyLevel && <p className="text-red-500 text-xs mt-1">{errors.hierarchyLevel}</p>}
                                    </div>

                                    {/* Permissões */}
                                    <div>
                                        <label className="block text-sm font-medium text-slate-700 mb-2">
                                            Permissões
                                        </label>
                                        <div className="space-y-2 bg-slate-50 rounded-lg p-3">
                                            <label className="flex items-center gap-2 cursor-pointer">
                                                <input
                                                    type="checkbox"
                                                    className="form-checkbox"
                                                    checked={canApproveRegistrations}
                                                    onChange={(e) => setCanApproveRegistrations(e.target.checked)}
                                                />
                                                <span className="text-sm">Aprovar cadastros de funcionários</span>
                                            </label>
                                            <label className="flex items-center gap-2 cursor-pointer">
                                                <input
                                                    type="checkbox"
                                                    className="form-checkbox"
                                                    checked={canCreateEmployees}
                                                    onChange={(e) => setCanCreateEmployees(e.target.checked)}
                                                />
                                                <span className="text-sm">Criar funcionários</span>
                                            </label>
                                            <label className="flex items-center gap-2 cursor-pointer">
                                                <input
                                                    type="checkbox"
                                                    className="form-checkbox"
                                                    checked={canEditEmployees}
                                                    onChange={(e) => setCanEditEmployees(e.target.checked)}
                                                />
                                                <span className="text-sm">Editar funcionários</span>
                                            </label>
                                            <label className="flex items-center gap-2 cursor-pointer">
                                                <input
                                                    type="checkbox"
                                                    className="form-checkbox"
                                                    checked={canDeleteEmployees}
                                                    onChange={(e) => setCanDeleteEmployees(e.target.checked)}
                                                />
                                                <span className="text-sm">Excluir funcionários</span>
                                            </label>
                                            <label className="flex items-center gap-2 cursor-pointer">
                                                <input
                                                    type="checkbox"
                                                    className="form-checkbox"
                                                    checked={canManageRoles}
                                                    onChange={(e) => setCanManageRoles(e.target.checked)}
                                                />
                                                <span className="text-sm">Gerenciar cargos</span>
                                            </label>
                                        </div>
                                    </div>

                                    {/* Footer */}
                                    <div className="flex justify-end gap-3 pt-4 border-t border-slate-200">
                                        <button
                                            type="button"
                                            className="btn btn-outline-dark"
                                            onClick={() => onClose(false)}
                                            disabled={saving}
                                        >
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
                                            ) : isEditing ? 'Salvar Alterações' : 'Criar Cargo'}
                                        </button>
                                    </div>
                                </form>
                            </Dialog.Panel>
                        </Transition.Child>
                    </div>
                </div>
            </Dialog>
        </Transition>
    );
};

export default RoleModal;

