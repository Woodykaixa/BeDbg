import { defineStore } from 'pinia';

type ReadyState = 'ready' | 'loading' | 'error';

export const loadingStateKeys = ['disassemblyState', 'panelState'] as const;
export type LoadingStateKey = typeof loadingStateKeys[number];

type InjectLoadingState<T> = {
  [K in LoadingStateKey]: ReadyState;
} & T;

export const useLoadingStates = defineStore('loadingStates', {
  state: () => {
    let state = {
      debuggerReady: false,
    };
    type FinalState = InjectLoadingState<typeof state>;
    loadingStateKeys.forEach(key => {
      // @ts-ignore
      state[key] = 'loading' as ReadyState;
    });
    return state as FinalState;
  },
  actions: {
    setAllLoading() {
      loadingStateKeys.forEach(key => {
        this[key] = 'loading';
      });
    },
    backupLoadingStates() {
      let backup: Record<LoadingStateKey, ReadyState> = {} as Record<LoadingStateKey, ReadyState>;
      loadingStateKeys.forEach(key => {
        backup[key] = this[key];
      });
      return backup;
    },
    restoreLoadingStates(backup: Record<LoadingStateKey, ReadyState>) {
      loadingStateKeys.forEach(key => {
        this[key] = backup[key];
      });
    },
  },
});
