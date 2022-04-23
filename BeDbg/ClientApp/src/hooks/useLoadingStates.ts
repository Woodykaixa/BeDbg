import { defineStore } from 'pinia';

type ReadyState = 'ready' | 'loading' | 'error';

export const useLoadingStates = defineStore('loadingStates', {
  state: () => {
    return {
      debuggerReady: false,
      disassemblyState: 'loading' as ReadyState,
    };
  },
});
