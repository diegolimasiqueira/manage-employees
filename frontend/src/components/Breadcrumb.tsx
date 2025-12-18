import { Link } from 'react-router-dom';
import IconHome from './Icon/IconHome';

interface BreadcrumbItem {
    label: string;
    path?: string;
}

interface BreadcrumbProps {
    items: BreadcrumbItem[];
    title?: string;
}

const Breadcrumb = ({ items, title }: BreadcrumbProps) => {
    return (
        <div className="mb-5">
            {title && (
                <h1 className="text-2xl font-bold text-slate-900 dark:text-white mb-2">{title}</h1>
            )}
            <nav className="flex" aria-label="Breadcrumb">
                <ol className="inline-flex items-center space-x-1 md:space-x-2 rtl:space-x-reverse">
                    <li className="inline-flex items-center">
                        <Link
                            to="/"
                            className="inline-flex items-center text-sm font-medium text-slate-500 hover:text-primary dark:text-slate-400 dark:hover:text-white transition-colors"
                        >
                            <IconHome className="w-4 h-4 ltr:mr-1.5 rtl:ml-1.5" />
                            Início
                        </Link>
                    </li>
                    {items.map((item, index) => (
                        <li key={index} className="inline-flex items-center">
                            <svg
                                className="w-3 h-3 text-slate-400 mx-1 rtl:rotate-180"
                                aria-hidden="true"
                                xmlns="http://www.w3.org/2000/svg"
                                fill="none"
                                viewBox="0 0 6 10"
                            >
                                <path
                                    stroke="currentColor"
                                    strokeLinecap="round"
                                    strokeLinejoin="round"
                                    strokeWidth="2"
                                    d="m1 9 4-4-4-4"
                                />
                            </svg>
                            {item.path && index < items.length - 1 ? (
                                <Link
                                    to={item.path}
                                    className="text-sm font-medium text-slate-500 hover:text-primary dark:text-slate-400 dark:hover:text-white transition-colors"
                                >
                                    {item.label}
                                </Link>
                            ) : (
                                <span className="text-sm font-medium text-slate-900 dark:text-white">
                                    {item.label}
                                </span>
                            )}
                        </li>
                    ))}
                </ol>
            </nav>
        </div>
    );
};

export default Breadcrumb;

