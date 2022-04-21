import { MemoryProtection } from '@/dto/process';
const FillZero = '0000000000000000';
export const DataFormatter = {
  formatNumberHex: (num: number | number[], count = 16) => {
    const nums = Array.isArray(num) ? num : [num];
    return nums.reduce((acc, cur) => {
      const hex = cur.toString(16);
      const comb = FillZero + num.toString(16);
      return acc + comb.slice(-count);
    }, '0x');
  },
  formatProtectionFlag: (protectionFlags: MemoryProtection) => {
    return `${protectionFlags.execute ? 'E' : '-'}${protectionFlags.read ? 'R' : '-'}${
      protectionFlags.write ? 'W' : '-'
    }${protectionFlags.copy ? 'C' : '-'}${protectionFlags.guard ? 'G' : '-'}`;
  },
};
