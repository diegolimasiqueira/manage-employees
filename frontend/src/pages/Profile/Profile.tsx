import { useEffect, useState } from 'react';
import { useDispatch } from 'react-redux';
import { setPageTitle } from '../../store/themeConfigSlice';
import {
    getCurrentUser,
    getEmployeeById,
    updateProfile,
    changePassword,
    uploadPhoto,
    deletePhoto,
    EmployeeDto,
    PhoneDto,
} from '../../services/api';
import Swal from 'sweetalert2';
import Breadcrumb from '../../components/Breadcrumb';
import IconUser from '../../components/Icon/IconUser';
import IconMail from '../../components/Icon/IconMail';
import IconPhone from '../../components/Icon/IconPhone';
import IconLockDots from '../../components/Icon/IconLockDots';
import IconPlus from '../../components/Icon/IconPlus';
import IconTrash from '../../components/Icon/IconTrash';

interface PhoneField {
    id: string;
    number: string;
    type: string;
}

const Profile = () => {
    const dispatch = useDispatch();
    const currentUser = getCurrentUser();

    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [savingPassword, setSavingPassword] = useState(false);
    const [profile, setProfile] = useState<EmployeeDto | null>(null);

    // Form de perfil
    const [name, setName] = useState('');
    const [email, setEmail] = useState('');
    const [phones, setPhones] = useState<PhoneField[]>([]);
    const [profileErrors, setProfileErrors] = useState<Record<string, string>>({});
    const [photoFile, setPhotoFile] = useState<File | null>(null);
    const [photoPreview, setPhotoPreview] = useState<string | null>(null);
    const [currentPhotoUrl, setCurrentPhotoUrl] = useState<string | null>(null);
    const [uploadingPhoto, setUploadingPhoto] = useState(false);

    // Form de senha
    const [currentPassword, setCurrentPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [passwordErrors, setPasswordErrors] = useState<Record<string, string>>({});

    useEffect(() => {
        dispatch(setPageTitle('Meu Perfil'));
        loadProfile();
    }, [dispatch]);

    const loadProfile = async () => {
        try {
            setLoading(true);
            const data = await getEmployeeById(currentUser!.id);
            setProfile(data);
            setName(data.name);
            setEmail(data.email);
            setPhones(
                data.phones.map((p, idx) => ({
                    id: p.id || `phone-${idx}`,
                    number: formatPhone(p.number),
                    type: p.type,
                }))
            );
            
            if (data.photoUrl) {
                setCurrentPhotoUrl(`http://localhost:5000${data.photoUrl}`);
            }
        } catch (error: any) {
            Swal.fire({
                icon: 'error',
                title: 'Erro',
                text: error.message || 'Erro ao carregar perfil',
                confirmButtonColor: '#006B3F',
            });
        } finally {
            setLoading(false);
        }
    };

    const formatPhone = (value: string) => {
        const numbers = value.replace(/\D/g, '');
        if (numbers.length <= 10) {
            return numbers.replace(/(\d{2})(\d{4})(\d{4})/, '($1) $2-$3');
        }
        return numbers.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
    };

    const handlePhoneChange = (index: number, value: string) => {
        const numbers = value.replace(/\D/g, '').slice(0, 11);
        const newPhones = [...phones];
        newPhones[index].number = formatPhone(numbers);
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

    const handlePhotoChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
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

            // Upload direto ao selecionar
            setUploadingPhoto(true);
            try {
                const newUrl = await uploadPhoto(currentUser!.id, file);
                setCurrentPhotoUrl(`http://localhost:5000${newUrl}`);
                setPhotoPreview(null);
                Swal.fire({
                    icon: 'success',
                    title: 'Foto atualizada',
                    text: 'Sua foto foi atualizada com sucesso.',
                    confirmButtonColor: '#006B3F',
                    timer: 2000,
                });
            } catch (error: any) {
                Swal.fire({
                    icon: 'error',
                    title: 'Erro',
                    text: error.message || 'Erro ao fazer upload da foto',
                    confirmButtonColor: '#006B3F',
                });
            } finally {
                setUploadingPhoto(false);
            }
        }
    };

    const handleRemovePhoto = async () => {
        const confirm = await Swal.fire({
            icon: 'question',
            title: 'Remover foto?',
            text: 'Deseja realmente remover sua foto de perfil?',
            showCancelButton: true,
            confirmButtonText: 'Sim, remover',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#e7515a',
            cancelButtonColor: '#6b7280',
        });

        if (confirm.isConfirmed) {
            setUploadingPhoto(true);
            try {
                await deletePhoto(currentUser!.id);
                setCurrentPhotoUrl(null);
                setPhotoPreview(null);
                Swal.fire({
                    icon: 'success',
                    title: 'Foto removida',
                    text: 'Sua foto foi removida com sucesso.',
                    confirmButtonColor: '#006B3F',
                    timer: 2000,
                });
            } catch (error: any) {
                Swal.fire({
                    icon: 'error',
                    title: 'Erro',
                    text: error.message || 'Erro ao remover foto',
                    confirmButtonColor: '#006B3F',
                });
            } finally {
                setUploadingPhoto(false);
            }
        }
    };

    const validateProfile = (): boolean => {
        const errors: Record<string, string> = {};

        if (!name.trim()) {
            errors.name = 'Nome é obrigatório';
        } else if (name.trim().split(' ').length < 2) {
            errors.name = 'Informe nome e sobrenome';
        }

        if (!email.trim()) {
            errors.email = 'E-mail é obrigatório';
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
            errors.email = 'E-mail inválido';
        }

        const validPhones = phones.filter(p => p.number.replace(/\D/g, '').length >= 10);
        if (validPhones.length === 0) {
            errors.phones = 'Informe pelo menos um telefone válido';
        }

        setProfileErrors(errors);
        return Object.keys(errors).length === 0;
    };

    const handleSaveProfile = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!validateProfile()) {
            return;
        }

        setSaving(true);

        try {
            await updateProfile({
                name: name.trim(),
                email: email.trim(),
                phones: phones
                    .filter(p => p.number.replace(/\D/g, '').length >= 10)
                    .map(p => ({
                        number: p.number.replace(/\D/g, ''),
                        type: p.type,
                    })),
            });

            await Swal.fire({
                icon: 'success',
                title: 'Sucesso!',
                text: 'Perfil atualizado com sucesso.',
                confirmButtonColor: '#006B3F',
            });

            loadProfile();
        } catch (error: any) {
            Swal.fire({
                icon: 'error',
                title: 'Erro',
                text: error.message || 'Erro ao atualizar perfil',
                confirmButtonColor: '#006B3F',
            });
        } finally {
            setSaving(false);
        }
    };

    const validatePassword = (): boolean => {
        const errors: Record<string, string> = {};

        if (!currentPassword) {
            errors.currentPassword = 'Senha atual é obrigatória';
        }

        if (!newPassword) {
            errors.newPassword = 'Nova senha é obrigatória';
        } else if (newPassword.length < 8) {
            errors.newPassword = 'A senha deve ter pelo menos 8 caracteres';
        }

        if (!confirmPassword) {
            errors.confirmPassword = 'Confirmação é obrigatória';
        } else if (newPassword !== confirmPassword) {
            errors.confirmPassword = 'As senhas não coincidem';
        }

        setPasswordErrors(errors);
        return Object.keys(errors).length === 0;
    };

    const handleChangePassword = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!validatePassword()) {
            return;
        }

        setSavingPassword(true);

        try {
            await changePassword({
                currentPassword,
                newPassword,
                confirmPassword,
            });

            await Swal.fire({
                icon: 'success',
                title: 'Sucesso!',
                text: 'Senha alterada com sucesso.',
                confirmButtonColor: '#006B3F',
            });

            // Limpar campos
            setCurrentPassword('');
            setNewPassword('');
            setConfirmPassword('');
            setPasswordErrors({});
        } catch (error: any) {
            Swal.fire({
                icon: 'error',
                title: 'Erro',
                text: error.message || 'Erro ao alterar senha',
                confirmButtonColor: '#006B3F',
            });
        } finally {
            setSavingPassword(false);
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
        <div className="max-w-4xl mx-auto">
            <Breadcrumb 
                items={[
                    { label: 'Meu Perfil' }
                ]} 
            />

            {/* Header com Foto */}
            <div className="panel border border-slate-200 mb-5">
                <div className="flex items-center gap-6">
                    <div className="relative">
                        {currentPhotoUrl ? (
                            <img
                                src={currentPhotoUrl}
                                alt="Foto de perfil"
                                className="w-24 h-24 rounded-full object-cover border-2 border-slate-200"
                            />
                        ) : (
                            <div className="w-24 h-24 rounded-full bg-slate-900 flex items-center justify-center text-white text-3xl font-bold">
                                {profile?.name?.charAt(0).toUpperCase()}
                            </div>
                        )}
                        {uploadingPhoto && (
                            <div className="absolute inset-0 bg-black/50 rounded-full flex items-center justify-center">
                                <div className="animate-spin rounded-full h-6 w-6 border-2 border-white border-t-transparent"></div>
                            </div>
                        )}
                    </div>
                    <div className="flex-1">
                        <h2 className="text-2xl font-bold text-slate-900">{profile?.name}</h2>
                        <p className="text-slate-500">{profile?.email}</p>
                        <div className="flex items-center gap-2 mt-1">
                            <span className="text-xs font-medium bg-slate-100 text-slate-700 px-2 py-0.5 rounded">{profile?.role.name}</span>
                            {profile?.enabled ? (
                                <span className="text-xs font-medium bg-emerald-50 text-emerald-700 px-2 py-0.5 rounded">Ativo</span>
                            ) : (
                                <span className="text-xs font-medium bg-amber-50 text-amber-700 px-2 py-0.5 rounded">Pendente</span>
                            )}
                        </div>
                        <div className="flex items-center gap-2 mt-3">
                            <label className="btn btn-outline-dark btn-sm cursor-pointer">
                                <input
                                    type="file"
                                    accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
                                    onChange={handlePhotoChange}
                                    className="hidden"
                                    disabled={uploadingPhoto}
                                />
                                {currentPhotoUrl ? 'Alterar Foto' : 'Adicionar Foto'}
                            </label>
                            {currentPhotoUrl && (
                                <button
                                    type="button"
                                    onClick={handleRemovePhoto}
                                    className="btn btn-outline-danger btn-sm"
                                    disabled={uploadingPhoto}
                                >
                                    Remover
                                </button>
                            )}
                        </div>
                    </div>
                </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
                {/* Dados Pessoais */}
                <div className="panel border border-slate-200">
                    <div className="flex items-center gap-2 mb-5">
                        <IconUser className="w-5 h-5 text-slate-600" />
                        <h5 className="text-lg font-semibold text-slate-900">Dados Pessoais</h5>
                    </div>

                    <form onSubmit={handleSaveProfile} className="space-y-4" noValidate>
                        <div>
                            <label htmlFor="name" className="block text-xs text-slate-500 uppercase tracking-wide mb-2">
                                Nome Completo
                            </label>
                            <input
                                id="name"
                                type="text"
                                className={`form-input border-slate-200 focus:border-primary ${profileErrors.name ? 'border-red-500' : ''}`}
                                value={name}
                                onChange={(e) => setName(e.target.value)}
                            />
                            {profileErrors.name && <p className="text-red-500 text-xs mt-1">{profileErrors.name}</p>}
                        </div>

                        <div>
                            <label htmlFor="email" className="block text-xs text-slate-500 uppercase tracking-wide mb-2">
                                E-mail
                            </label>
                            <input
                                id="email"
                                type="email"
                                className={`form-input border-slate-200 focus:border-primary ${profileErrors.email ? 'border-red-500' : ''}`}
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                            />
                            {profileErrors.email && <p className="text-red-500 text-xs mt-1">{profileErrors.email}</p>}
                        </div>

                        {/* Telefones */}
                        <div>
                            <div className="flex items-center justify-between mb-2">
                                <label className="block text-xs text-slate-500 uppercase tracking-wide">
                                    Telefones
                                </label>
                                <button
                                    type="button"
                                    onClick={addPhone}
                                    className="btn btn-sm btn-outline-dark py-1 px-2"
                                >
                                    <IconPlus className="w-4 h-4" />
                                </button>
                            </div>
                            
                            {profileErrors.phones && (
                                <p className="text-red-500 text-xs mb-2">{profileErrors.phones}</p>
                            )}

                            <div className="space-y-2">
                                {phones.map((phone, index) => (
                                    <div key={phone.id} className="flex gap-2">
                                        <input
                                            type="text"
                                            className="form-input border-slate-200 focus:border-primary flex-1"
                                            placeholder="(00) 00000-0000"
                                            value={phone.number}
                                            onChange={(e) => handlePhoneChange(index, e.target.value)}
                                        />
                                        <select
                                            className="form-select border-slate-200 w-28"
                                            value={phone.type}
                                            onChange={(e) => handlePhoneTypeChange(index, e.target.value)}
                                        >
                                            <option value="Mobile">Celular</option>
                                            <option value="Home">Casa</option>
                                            <option value="Work">Trabalho</option>
                                        </select>
                                        <button
                                            type="button"
                                            onClick={() => removePhone(index)}
                                            className="btn btn-sm btn-outline-dark px-2"
                                        >
                                            <IconTrash className="w-4 h-4" />
                                        </button>
                                    </div>
                                ))}
                            </div>
                        </div>

                        {/* Campos somente leitura */}
                        <div className="pt-4 border-t border-slate-200">
                            <p className="text-xs text-slate-500 mb-3">
                                Os campos abaixo são gerenciados pelo seu gestor
                            </p>
                            <div className="grid grid-cols-2 gap-4 text-sm">
                                <div>
                                    <span className="text-slate-500">CPF:</span>
                                    <p className="font-medium">{profile?.documentNumber}</p>
                                </div>
                                <div>
                                    <span className="text-slate-500">Cargo:</span>
                                    <p className="font-medium">{profile?.role.name}</p>
                                </div>
                                <div>
                                    <span className="text-slate-500">Gerente:</span>
                                    <p className="font-medium">{profile?.managerName || 'Nenhum'}</p>
                                </div>
                                <div>
                                    <span className="text-slate-500">Nascimento:</span>
                                    <p className="font-medium">
                                        {profile && new Date(profile.birthDate).toLocaleDateString('pt-BR')}
                                    </p>
                                </div>
                            </div>
                        </div>

                        <button
                            type="submit"
                            className="btn btn-primary w-full"
                            disabled={saving}
                        >
                            {saving ? 'Salvando...' : 'Salvar Alterações'}
                        </button>
                    </form>
                </div>

                {/* Segurança */}
                <div className="panel border border-slate-200">
                    <div className="flex items-center gap-2 mb-5">
                        <IconLockDots className="w-5 h-5 text-slate-600" />
                        <h5 className="text-lg font-semibold text-slate-900">Segurança</h5>
                    </div>

                    <form onSubmit={handleChangePassword} className="space-y-4" noValidate>
                        <div>
                            <label htmlFor="currentPassword" className="block text-xs text-slate-500 uppercase tracking-wide mb-2">
                                Senha Atual
                            </label>
                            <input
                                id="currentPassword"
                                type="password"
                                className={`form-input border-slate-200 focus:border-primary ${passwordErrors.currentPassword ? 'border-red-500' : ''}`}
                                value={currentPassword}
                                onChange={(e) => setCurrentPassword(e.target.value)}
                            />
                            {passwordErrors.currentPassword && (
                                <p className="text-red-500 text-xs mt-1">{passwordErrors.currentPassword}</p>
                            )}
                        </div>

                        <div>
                            <label htmlFor="newPassword" className="block text-xs text-slate-500 uppercase tracking-wide mb-2">
                                Nova Senha
                            </label>
                            <input
                                id="newPassword"
                                type="password"
                                className={`form-input border-slate-200 focus:border-primary ${passwordErrors.newPassword ? 'border-red-500' : ''}`}
                                value={newPassword}
                                onChange={(e) => setNewPassword(e.target.value)}
                            />
                            {passwordErrors.newPassword && (
                                <p className="text-red-500 text-xs mt-1">{passwordErrors.newPassword}</p>
                            )}
                        </div>

                        <div>
                            <label htmlFor="confirmPassword" className="block text-xs text-slate-500 uppercase tracking-wide mb-2">
                                Confirmar Nova Senha
                            </label>
                            <input
                                id="confirmPassword"
                                type="password"
                                className={`form-input border-slate-200 focus:border-primary ${passwordErrors.confirmPassword ? 'border-red-500' : ''}`}
                                value={confirmPassword}
                                onChange={(e) => setConfirmPassword(e.target.value)}
                            />
                            {passwordErrors.confirmPassword && (
                                <p className="text-red-500 text-xs mt-1">{passwordErrors.confirmPassword}</p>
                            )}
                        </div>

                        <button
                            type="submit"
                            className="btn btn-outline-dark w-full"
                            disabled={savingPassword}
                        >
                            {savingPassword ? 'Alterando...' : 'Alterar Senha'}
                        </button>
                    </form>
                </div>
            </div>
        </div>
    );
};

export default Profile;

