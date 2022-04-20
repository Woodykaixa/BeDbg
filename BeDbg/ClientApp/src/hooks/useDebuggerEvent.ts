import { DebuggerEventSource } from '@/util/debuggerEventSource';
import { provide, inject } from 'vue';

export const provideDebuggerEventSource = (eventSource: DebuggerEventSource) => {
  provide('debuggerEventSource', eventSource);
};

export const useDebuggerEventSource = () => {
  const eventSource = inject<DebuggerEventSource>('debuggerEventSource');

  return eventSource!;
};
