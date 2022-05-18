/// <reference types="vite/client" />

declare module '*.vue' {
  import type { DefineComponent } from 'vue';
  // eslint-disable-next-line @typescript-eslint/no-explicit-any, @typescript-eslint/ban-types
  const component: DefineComponent<{}, {}, any>;
  export default component;
}

interface Window {
  beDbg: {
    installPlugin: (plugin: any) => void;
    plugin: Record<
      string,
      {
        commands?: Record<string, Function>;
        output: object[];
        error: object[];
      }
    >;
  };
}
