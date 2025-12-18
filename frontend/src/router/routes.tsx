import { lazy } from 'react';
import PrivateRoute from '../components/PrivateRoute';

// Páginas
const Index = lazy(() => import('../pages/Index'));
const LoginBoxed = lazy(() => import('../pages/Authentication/LoginBoxed'));
const RegisterBoxed = lazy(() => import('../pages/Authentication/RegisterBoxed'));
const EmployeeList = lazy(() => import('../pages/Employees/EmployeeList'));
const EmployeeCreate = lazy(() => import('../pages/Employees/EmployeeCreate'));
const EmployeeEdit = lazy(() => import('../pages/Employees/EmployeeEdit'));
const RoleList = lazy(() => import('../pages/Roles/RoleList'));
const Profile = lazy(() => import('../pages/Profile/Profile'));
const Error = lazy(() => import('../components/Error'));

const routes = [
    // Dashboard
    {
        path: '/',
        element: (
            <PrivateRoute>
                <Index />
            </PrivateRoute>
        ),
        layout: 'default',
    },
    // Funcionários - Listagem
    {
        path: '/employees',
        element: (
            <PrivateRoute>
                <EmployeeList />
            </PrivateRoute>
        ),
        layout: 'default',
    },
    // Funcionários - Criação
    {
        path: '/employees/create',
        element: (
            <PrivateRoute requiredPermission="canCreateEmployees">
                <EmployeeCreate />
            </PrivateRoute>
        ),
        layout: 'default',
    },
    // Funcionários - Edição
    {
        path: '/employees/edit/:id',
        element: (
            <PrivateRoute requiredPermission="canEditEmployees">
                <EmployeeEdit />
            </PrivateRoute>
        ),
        layout: 'default',
    },
    // Cargos - Listagem
    {
        path: '/roles',
        element: (
            <PrivateRoute>
                <RoleList />
            </PrivateRoute>
        ),
        layout: 'default',
    },
    // Perfil do usuário
    {
        path: '/profile',
        element: (
            <PrivateRoute>
                <Profile />
            </PrivateRoute>
        ),
        layout: 'default',
    },
    // Autenticação - Login
    {
        path: '/auth/boxed-signin',
        element: <LoginBoxed />,
        layout: 'blank',
    },
    {
        path: '/auth/login',
        element: <LoginBoxed />,
        layout: 'blank',
    },
    // Autenticação - Registro
    {
        path: '/auth/boxed-signup',
        element: <RegisterBoxed />,
        layout: 'blank',
    },
    {
        path: '/auth/register',
        element: <RegisterBoxed />,
        layout: 'blank',
    },
    // Erro 404
    {
        path: '*',
        element: <Error />,
        layout: 'blank',
    },
];

export { routes };
