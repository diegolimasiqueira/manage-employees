import { createSlice } from '@reduxjs/toolkit';
import i18next from 'i18next';
import themeConfig from '../theme.config';

// FORÇAR tema light - remover localStorage antigo se existir com valor diferente
const savedTheme = localStorage.getItem('theme');
if (savedTheme !== 'light') {
    localStorage.setItem('theme', 'light');
}

// Garantir que o tema light está aplicado - remover classe dark
document.querySelector('body')?.classList.remove('dark');
document.documentElement.classList.remove('dark');

const initialState = {
    theme: 'light',
    menu: localStorage.getItem('menu') || themeConfig.menu,
    layout: localStorage.getItem('layout') || themeConfig.layout,
    rtlClass: localStorage.getItem('rtlClass') || themeConfig.rtlClass,
    animation: localStorage.getItem('animation') || themeConfig.animation,
    navbar: localStorage.getItem('navbar') || themeConfig.navbar,
    locale: localStorage.getItem('i18nextLng') || themeConfig.locale,
    isDarkMode: false,
    sidebar: localStorage.getItem('sidebar') === 'true' || false,
    semidark: localStorage.getItem('semidark') === 'true' || themeConfig.semidark,
    languageList: themeConfig.languageList || [
        { code: 'pt', name: 'Português' },
        { code: 'en', name: 'English' },
        { code: 'es', name: 'Español' },
    ],
    pageTitle: '',
};

const themeConfigSlice = createSlice({
    name: 'themeConfig',
    initialState: initialState,
    reducers: {
        toggleTheme(state, { payload }) {
            // Ignorar qualquer tentativa de mudar o tema - sempre light
            state.theme = 'light';
                state.isDarkMode = false;
            localStorage.setItem('theme', 'light');
                document.querySelector('body')?.classList.remove('dark');
            document.documentElement.classList.remove('dark');
        },
        toggleMenu(state, { payload }) {
            payload = payload || state.menu;
            state.sidebar = false;
            localStorage.setItem('menu', payload);
            state.menu = payload;
        },
        toggleLayout(state, { payload }) {
            payload = payload || state.layout;
            localStorage.setItem('layout', payload);
            state.layout = payload;
        },
        toggleRTL(state, { payload }) {
            payload = payload || state.rtlClass;
            localStorage.setItem('rtlClass', payload);
            state.rtlClass = payload;
            document.querySelector('html')?.setAttribute('dir', state.rtlClass || 'ltr');
        },
        toggleAnimation(state, { payload }) {
            payload = payload || state.animation;
            payload = payload?.trim();
            localStorage.setItem('animation', payload);
            state.animation = payload;
        },
        toggleNavbar(state, { payload }) {
            payload = payload || state.navbar;
            localStorage.setItem('navbar', payload);
            state.navbar = payload;
        },
        toggleSemidark(state, { payload }) {
            payload = payload === true || payload === 'true' ? true : false;
            localStorage.setItem('semidark', String(payload));
            state.semidark = payload;
        },
        toggleLocale(state, { payload }) {
            payload = payload || state.locale;
            i18next.changeLanguage(payload);
            state.locale = payload;
        },
        toggleSidebar(state) {
            state.sidebar = !state.sidebar;
        },
        setPageTitle(state, { payload }) {
            state.pageTitle = payload;
            document.title = `${payload} | Gestão de Funcionários`;
        },
    },
});

export const { 
    toggleTheme, 
    toggleMenu, 
    toggleLayout, 
    toggleRTL, 
    toggleAnimation, 
    toggleNavbar, 
    toggleSemidark, 
    toggleLocale, 
    toggleSidebar, 
    setPageTitle 
} = themeConfigSlice.actions;

export default themeConfigSlice.reducer;
