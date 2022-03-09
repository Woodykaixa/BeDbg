import { createApp } from 'vue';
import Index from './pages/Index.vue';
import App from './App.vue';
import 'vfonts/Lato.css';
import 'vfonts/FiraCode.css';
import { createRouter, createWebHistory } from 'vue-router';

createApp(App)
  .use(
    createRouter({
      history: createWebHistory(),
      routes: [
        {
          path: '/',
          component: Index,
        },
      ],
    })
  )
  .mount('#app');
