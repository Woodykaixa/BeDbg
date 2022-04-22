import { createApp } from 'vue';
import Index from './pages/Index.vue';
import Debug from '@/pages/Debug.vue';
import App from './App.vue';
import 'vfonts/Lato.css';
import 'vfonts/FiraCode.css';
import { createRouter, createWebHistory } from 'vue-router';
import { createPinia } from 'pinia';

createApp(App)
  .use(createPinia())
  .use(
    createRouter({
      history: createWebHistory(),
      routes: [
        {
          path: '/',
          component: Index,
        },
        {
          path: '/debug',
          name: 'debug',
          component: Debug,
        },
      ],
    })
  )
  .mount('#app');
