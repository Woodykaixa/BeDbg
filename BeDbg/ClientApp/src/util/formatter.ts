import { MemoryProtection } from '@/dto/process';

export const DataFormatter = {
  formatNumberHex: (num: number) => {
    const comb = '0000000000000000' + num.toString(16);
    return '0x' + comb.slice(-16);
  },
  formatProtectionFlag: (protectionFlags: MemoryProtection) => {
    return `${protectionFlags.execute ? 'E' : '-'}${protectionFlags.read ? 'R' : '-'}${
      protectionFlags.write ? 'W' : '-'
    }${protectionFlags.copy ? 'C' : '-'}${protectionFlags.guard ? 'G' : '-'}`;
  },
};
