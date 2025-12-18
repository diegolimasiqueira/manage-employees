import { Navigate, useLocation } from 'react-router-dom';
import { isAuthenticated, getCurrentUser, UserInfo } from '../services/api';

interface PrivateRouteProps {
    children: React.ReactNode;
    requiredPermission?: keyof Pick<UserInfo, 'canApproveRegistrations' | 'canCreateEmployees' | 'canEditEmployees' | 'canDeleteEmployees' | 'canManageRoles'>;
}

const PrivateRoute = ({ children, requiredPermission }: PrivateRouteProps) => {
    const location = useLocation();
    
    // Verificar se está autenticado
    if (!isAuthenticated()) {
        return <Navigate to="/auth/login" state={{ from: location }} replace />;
    }
    
    // Verificar permissão específica se necessário
    if (requiredPermission) {
        const user = getCurrentUser();
        if (!user || !user[requiredPermission]) {
            return <Navigate to="/" replace />;
        }
    }
    
    return <>{children}</>;
};

export default PrivateRoute;

