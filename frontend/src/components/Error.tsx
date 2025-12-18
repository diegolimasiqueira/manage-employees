import { Link } from 'react-router-dom';
import { useEffect } from 'react';
import { useDispatch } from 'react-redux';
import { setPageTitle } from '../store/themeConfigSlice';

const Error = () => {
    const dispatch = useDispatch();
    
    useEffect(() => {
        dispatch(setPageTitle('Erro 404'));
    }, [dispatch]);

    return (
        <div className="relative flex min-h-screen items-center justify-center overflow-hidden">
            <div className="px-6 py-16 text-center font-semibold before:container before:absolute before:left-1/2 before:-translate-x-1/2 before:rounded-full before:bg-gradient-to-b before:from-primary before:to-transparent before:aspect-square before:opacity-10 md:py-20">
                <div className="relative">
                    <img
                        src="/assets/images/error/404-dark.svg"
                        alt="404"
                        className="mx-auto -mt-10 w-full max-w-xs object-cover md:-mt-14 md:max-w-xl dark:block hidden"
                    />
                    <img
                        src="/assets/images/error/404-light.svg"
                        alt="404"
                        className="mx-auto -mt-10 w-full max-w-xs object-cover md:-mt-14 md:max-w-xl dark:hidden"
                    />
                    <p className="mt-5 text-base dark:text-white">Página não encontrada!</p>
                    <Link to="/" className="btn btn-gradient mx-auto !mt-7 w-max border-0 uppercase shadow-none">
                        Voltar ao Início
                    </Link>
                </div>
            </div>
        </div>
    );
};

export default Error;
