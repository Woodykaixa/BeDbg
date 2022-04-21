import { Registers } from '@/dto/thread';
import { provide, inject, ref, Ref } from 'vue';

type RegistersFetcher = () => Promise<Registers>;

export const provideRegisters = (registers: Registers, fetcher: RegistersFetcher) => {
  const registerRef = ref(registers);

  const updateRegisters = async () => {
    const data = await fetcher();
    registerRef.value = data;
  };
  provide('registers', {
    registerRef,
    updateRegisters,
  });

  return [registerRef, updateRegisters] as const;
};

export const useRegisters = () => {
  const result = inject<{
    registers: Ref<Registers>;
    updateRegisters: () => Promise<void>;
  }>('registers');

  console.log('useReg', result);
  const { registers, updateRegisters } = result!;

  return [registers, updateRegisters] as const;
};
