<script lang="ts" setup>
import { DataFormatter } from '@/util/formatter';
import { Registers } from '@/dto/thread';
import { onUpdated, ref, PropType } from 'vue';
import { NDataTable, NCard, NScrollbar, NCollapse, NCollapseItem } from 'naive-ui';

const props = defineProps({
  registers: {
    type: Object as PropType<Registers>,
    required: true,
  },
});

const extractBits = (flag: number, bit: number, count: number = 1) => {
  const result = [] as ('0' | '1')[];
  for (let i = 0; i < count; i++) {
    const current = (flag & (1 << bit)) === 0 ? '0' : '1';
    result.unshift(current);
    bit++;
  }
  return '0b' + result.join('');
};

type RegData = { name: string; value: string };
type RegDataRange = RegData & { range: string };

type TwoCol<
  Data,
  OptionalTwo extends boolean = false,
  Key extends keyof Data = keyof Data,
  RegDataType extends Extract<Key, string> = Extract<Key, string>
> = {
  [key in `${RegDataType}1`]: key extends `${infer OriginKey}1`
    ? OriginKey extends Key
      ? Data[OriginKey]
      : never
    : never;
} & (OptionalTwo extends true
  ? {
      [key in `${RegDataType}2`]?: key extends `${infer OriginKey}2`
        ? OriginKey extends Key
          ? Data[OriginKey]
          : never
        : never;
    }
  : {
      [key in `${RegDataType}2`]: key extends `${infer OriginKey}2`
        ? OriginKey extends Key
          ? Data[OriginKey]
          : never
        : never;
    });

type DisplayRegisterType = {
  general: TwoCol<RegData, true>[];
  fpuTagWord: TwoCol<RegDataRange>[];
  fpu: (RegData & { content: number })[];
  fpuControlWord: TwoCol<RegDataRange>[];
  fpuStatusWord: TwoCol<RegDataRange>[];
  simd: RegData[];
  mxCsr: TwoCol<RegDataRange, true>[];
  segment: TwoCol<RegData>[];
  debug: TwoCol<RegData>[];
  eFlags: TwoCol<RegDataRange, true>[];
};

const formatRegisters = () => ({
  general: [
    {
      name1: 'RAX',
      value1: DataFormatter.formatNumberHex(props.registers.rax),
      name2: 'RBX',
      value2: DataFormatter.formatNumberHex(props.registers.rbx),
    },
    {
      name1: 'RCX',
      value1: DataFormatter.formatNumberHex(props.registers.rcx),
      name2: 'RDX',
      value2: DataFormatter.formatNumberHex(props.registers.rdx),
    },
    {
      name1: 'RSI',
      value1: DataFormatter.formatNumberHex(props.registers.rsi),
      name2: 'RDI',
      value2: DataFormatter.formatNumberHex(props.registers.rdi),
    },
    {
      name1: 'RSP',
      value1: DataFormatter.formatNumberHex(props.registers.rsp),
      name2: 'RBP',
      value2: DataFormatter.formatNumberHex(props.registers.rbp),
    },
    {
      name1: 'R8',
      value1: DataFormatter.formatNumberHex(props.registers.r8),
      name2: 'R9',
      value2: DataFormatter.formatNumberHex(props.registers.r9),
    },
    {
      name1: 'R10',
      value1: DataFormatter.formatNumberHex(props.registers.r10),
      name2: 'R11',
      value2: DataFormatter.formatNumberHex(props.registers.r11),
    },
    {
      name1: 'R12',
      value1: DataFormatter.formatNumberHex(props.registers.r12),
      name2: 'R13',
      value2: DataFormatter.formatNumberHex(props.registers.r13),
    },
    {
      name1: 'R14',
      value1: DataFormatter.formatNumberHex(props.registers.r14),
      name2: 'R15',
      value2: DataFormatter.formatNumberHex(props.registers.r15),
    },
    {
      name1: 'RIP',
      value1: DataFormatter.formatNumberHex(props.registers.rip),
    },
  ],
  fpuTagWord: [
    {
      name1: 'Tagword1',
      value1: extractBits(props.registers.fpuTagWord, 14, 2),
      range1: '15-14',
      name2: 'Tagword2',
      value2: extractBits(props.registers.fpuTagWord, 12, 2),
      range2: '13-12',
    },
    {
      name1: 'Tagword3',
      value1: extractBits(props.registers.fpuTagWord, 10, 2),
      range1: '11-10',
      name2: 'Tagword4',
      value2: extractBits(props.registers.fpuTagWord, 8, 2),
      range2: '9-8',
    },
    {
      name1: 'Tagword5',
      value1: extractBits(props.registers.fpuTagWord, 6, 2),
      range1: '7-6',
      name2: 'Tagword6',
      value2: extractBits(props.registers.fpuTagWord, 4, 2),
      range2: '5-4',
    },
    {
      name1: 'Tagword7',
      value1: extractBits(props.registers.fpuTagWord, 2, 2),
      range1: '3-2',
      name2: 'Tagword8',
      value2: extractBits(props.registers.fpuTagWord, 0, 2),
      range2: '1-0',
    },
  ],
  fpuControlWord: [
    {
      name1: 'RC',
      value1: extractBits(props.registers.fpuControlWord, 12, 2),
      range1: '13-12',
      name2: 'PC',
      value2: extractBits(props.registers.fpuControlWord, 6, 2),
      range2: '6-7',
    },
    {
      name1: 'PM',
      value1: extractBits(props.registers.fpuControlWord, 5, 1),
      range1: '5',
      name2: 'UM',
      value2: extractBits(props.registers.fpuControlWord, 4, 1),
      range2: '4',
    },
    {
      name1: 'OM',
      value1: extractBits(props.registers.fpuControlWord, 3, 1),
      range1: '3',
      name2: 'ZM',
      value2: extractBits(props.registers.fpuControlWord, 2, 1),
      range2: '2',
    },
    {
      name1: 'DM',
      value1: extractBits(props.registers.fpuControlWord, 1, 1),
      range1: '1',
      name2: 'IM',
      value2: extractBits(props.registers.fpuControlWord, 0, 1),
      range2: '0',
    },
  ],
  fpuStatusWord: [
    {
      name1: 'Top',
      value1: extractBits(props.registers.fpuStatusWord, 11, 4),
      range1: '14-11',
      name2: 'B',
      value2: extractBits(props.registers.fpuStatusWord, 15),
      range2: '15',
    },
    {
      name1: 'C0',
      value1: extractBits(props.registers.fpuStatusWord, 8),
      range1: '8',
      name2: 'C1',
      value2: extractBits(props.registers.fpuStatusWord, 9),
      range2: '9',
    },
    {
      name1: 'C2',
      value1: extractBits(props.registers.fpuStatusWord, 10),
      range1: '10',
      name2: 'C3',
      value2: extractBits(props.registers.fpuStatusWord, 14),
      range2: '14',
    },
    {
      name1: 'ES',
      value1: extractBits(props.registers.fpuStatusWord, 7),
      range1: '7',
      name2: 'SF',
      value2: extractBits(props.registers.fpuStatusWord, 6),
      range2: '6',
    },
    {
      name1: 'PE',
      value1: extractBits(props.registers.fpuStatusWord, 5),
      range1: '5',
      name2: 'UE',
      value2: extractBits(props.registers.fpuStatusWord, 4),
      range2: '4',
    },
    {
      name1: 'OE',
      value1: extractBits(props.registers.fpuStatusWord, 3),
      range1: '3',
      name2: 'ZE',
      value2: extractBits(props.registers.fpuStatusWord, 2),
      range2: '2',
    },
    {
      name1: 'DE',
      value1: extractBits(props.registers.fpuStatusWord, 1),
      range1: '1',
      name2: 'IE',
      value2: extractBits(props.registers.fpuStatusWord, 0),
      range2: '0',
    },
  ],
  fpu: [
    {
      name: 'ST0',
      value:
        DataFormatter.formatNumberHex(props.registers.st0.high, 4) +
        DataFormatter.convertDoubleToHex(props.registers.st0.low).slice(2),
      content: props.registers.st0.low,
    },
    {
      name: 'ST1',
      value:
        DataFormatter.formatNumberHex(props.registers.st1.high, 4) +
        DataFormatter.convertDoubleToHex(props.registers.st1.low).slice(2),
      content: props.registers.st1.low,
    },
    {
      name: 'ST2',
      value:
        DataFormatter.formatNumberHex(props.registers.st2.high, 4) +
        DataFormatter.convertDoubleToHex(props.registers.st2.low).slice(2),
      content: props.registers.st2.low,
    },
    {
      name: 'ST3',
      value:
        DataFormatter.formatNumberHex(props.registers.st3.high, 4) +
        DataFormatter.convertDoubleToHex(props.registers.st3.low).slice(2),
      content: props.registers.st3.low,
    },
    {
      name: 'ST4',
      value:
        DataFormatter.formatNumberHex(props.registers.st4.high, 4) +
        DataFormatter.convertDoubleToHex(props.registers.st4.low).slice(2),
      content: props.registers.st4.low,
    },
    {
      name: 'ST5',
      value:
        DataFormatter.formatNumberHex(props.registers.st5.high, 4) +
        DataFormatter.convertDoubleToHex(props.registers.st5.low).slice(2),
      content: props.registers.st5.low,
    },
    {
      name: 'ST6',
      value:
        DataFormatter.formatNumberHex(props.registers.st6.high, 4) +
        DataFormatter.convertDoubleToHex(props.registers.st6.low).slice(2),
      content: props.registers.st6.low,
    },
    {
      name: 'ST7',
      value:
        DataFormatter.formatNumberHex(props.registers.st7.high, 4) +
        DataFormatter.convertDoubleToHex(props.registers.st7.low).slice(2),
      content: props.registers.st7.low,
    },
  ],
  segment: [
    {
      name1: 'CS',
      value1: DataFormatter.formatNumberHex(props.registers.segCs, 4),
      name2: 'DS',
      value2: DataFormatter.formatNumberHex(props.registers.segDs, 4),
    },
    {
      name1: 'ES',
      value1: DataFormatter.formatNumberHex(props.registers.segEs, 4),
      name2: 'FS',
      value2: DataFormatter.formatNumberHex(props.registers.segFs, 4),
    },
    {
      name1: 'GS',
      value1: DataFormatter.formatNumberHex(props.registers.segGs, 4),
      name2: 'SS',
      value2: DataFormatter.formatNumberHex(props.registers.segSs, 4),
    },
  ],
  simd: [
    {
      name: 'XMM0',
      value: DataFormatter.formatNumberHex([props.registers.xmm0.high, props.registers.xmm0.low]),
    },
    {
      name: 'XMM1',
      value: DataFormatter.formatNumberHex([props.registers.xmm1.high, props.registers.xmm1.low]),
    },
    {
      name: 'XMM2',
      value: DataFormatter.formatNumberHex([props.registers.xmm2.high, props.registers.xmm2.low]),
    },
    {
      name: 'XMM3',
      value: DataFormatter.formatNumberHex([props.registers.xmm3.high, props.registers.xmm3.low]),
    },
    {
      name: 'XMM4',
      value: DataFormatter.formatNumberHex([props.registers.xmm4.high, props.registers.xmm4.low]),
    },
    {
      name: 'XMM5',
      value: DataFormatter.formatNumberHex([props.registers.xmm5.high, props.registers.xmm5.low]),
    },
    {
      name: 'XMM6',
      value: DataFormatter.formatNumberHex([props.registers.xmm6.high, props.registers.xmm6.low]),
    },
    {
      name: 'XMM7',
      value: DataFormatter.formatNumberHex([props.registers.xmm7.high, props.registers.xmm7.low]),
    },
  ],
  mxCsr: [
    {
      name1: 'FZ',
      value1: extractBits(props.registers.mxCsr, 15),
      range1: '15',
      name2: 'RC',
      value2: extractBits(props.registers.mxCsr, 13, 2),
      range2: '14-13',
    },
    {
      name1: 'PM',
      value1: extractBits(props.registers.mxCsr, 12),
      range1: '12',
      name2: 'UM',
      value2: extractBits(props.registers.mxCsr, 11),
      range2: '11',
    },
    {
      name1: 'OM',
      value1: extractBits(props.registers.mxCsr, 10),
      range1: '10',
      name2: 'ZM',
      value2: extractBits(props.registers.mxCsr, 9),
      range2: '9',
    },
    {
      name1: 'DM',
      value1: extractBits(props.registers.mxCsr, 8),
      range1: '8',
      name2: 'IM',
      value2: extractBits(props.registers.mxCsr, 7),
      range2: '7',
    },
    {
      name1: 'DAZ',
      value1: extractBits(props.registers.mxCsr, 6),
      range1: '6',
      name2: 'PE',
      value2: extractBits(props.registers.mxCsr, 5),
      range2: '5',
    },
    {
      name1: 'UE',
      value1: extractBits(props.registers.mxCsr, 4),
      range1: '4',
      name2: 'OE',
      value2: extractBits(props.registers.mxCsr, 3),
      range2: '3',
    },
    {
      name1: 'ZE',
      value1: extractBits(props.registers.mxCsr, 2),
      range1: '2',
      name2: 'DE',
      value2: extractBits(props.registers.mxCsr, 1),
      range2: '1',
    },
    {
      name1: 'IE',
      value1: extractBits(props.registers.mxCsr, 0),
      range1: '0',
    },
  ],
  debug: [
    {
      name1: 'DR0',
      value1: DataFormatter.formatNumberHex(props.registers.dr0),
      name2: 'DR1',
      value2: DataFormatter.formatNumberHex(props.registers.dr1),
    },
    {
      name1: 'DR2',
      value1: DataFormatter.formatNumberHex(props.registers.dr2),
      name2: 'DR3',
      value2: DataFormatter.formatNumberHex(props.registers.dr3),
    },
    {
      name1: 'DR6',
      value1: DataFormatter.formatNumberHex(props.registers.dr6),
      name2: 'DR7',
      value2: DataFormatter.formatNumberHex(props.registers.dr7),
    },
  ],
  eFlags: [
    {
      name1: 'CF',
      value1: extractBits(props.registers.eFlags, 0),
      range1: '0',
      name2: 'PF',
      value2: extractBits(props.registers.eFlags, 2),
      range2: '2',
    },
    {
      name1: 'AF',
      value1: extractBits(props.registers.eFlags, 4),
      range1: '4',
      name2: 'ZF',
      value2: extractBits(props.registers.eFlags, 6),
      range2: '6',
    },
    {
      name1: 'SF',
      value1: extractBits(props.registers.eFlags, 7),
      range1: '7',
      name2: 'TF',
      value2: extractBits(props.registers.eFlags, 8),
      range2: '8',
    },
    {
      name1: 'IF',
      value1: extractBits(props.registers.eFlags, 9),
      range1: '9',
      name2: 'DF',
      value2: extractBits(props.registers.eFlags, 10),
      range2: '10',
    },
    {
      name1: 'OF',
      value1: extractBits(props.registers.eFlags, 11),
      range1: '11',
      name2: 'IOPL',
      value2: extractBits(props.registers.eFlags, 12, 2),
      range2: '13-12',
    },
    {
      name1: 'NT',
      value1: extractBits(props.registers.eFlags, 14),
      range1: '14',
      name2: 'RF',
      value2: extractBits(props.registers.eFlags, 16),
      range2: '16',
    },
    {
      name1: 'VM',
      value1: extractBits(props.registers.eFlags, 17),
      range1: '17',
      name2: 'AC',
      value2: extractBits(props.registers.eFlags, 18),
      range2: '18',
    },
    {
      name1: 'VIF',
      value1: extractBits(props.registers.eFlags, 19),
      range1: '19',
      name2: 'VIP',
      value2: extractBits(props.registers.eFlags, 20),
      range2: '20',
    },
    { name1: 'ID', value1: extractBits(props.registers.eFlags, 21), range1: '21' },
  ],
});

const registerData = ref(formatRegisters());
onUpdated(() => {
  registerData.value = formatRegisters();
});
</script>

<template>
  <n-scrollbar>
    <n-card bordered title="寄存器">
      <n-collapse>
        <n-collapse-item title="通用寄存器">
          <n-data-table
            :bordered="false"
            :columns="[
              { title: '名称', key: 'name1' },
              { title: '值', key: 'value1' },
              { title: '名称', key: 'name2' },
              { title: '值', key: 'value2' },
            ]"
            :data="registerData.general"
          />
        </n-collapse-item>
        <n-collapse-item :title="`EFlags 标志寄存器: ${DataFormatter.formatNumberHex(props.registers.eFlags, 8)}`">
          <n-data-table
            :border="false"
            :columns="[
              { title: '名称', key: 'name1' },
              { title: '值', key: 'value1' },
              { title: '位置', key: 'range1' },
              { title: '名称', key: 'name2' },
              { title: '值', key: 'value2' },
              { title: '位置', key: 'range2' },
            ]"
            :data="registerData.eFlags"
          />
        </n-collapse-item>
        <n-collapse-item title="段寄存器">
          <n-data-table
            :bordered="false"
            :columns="[
              { title: '名称', key: 'name1' },
              { title: '值', key: 'value1' },
              { title: '名称', key: 'name2' },
              { title: '值', key: 'value2' },
            ]"
            :data="registerData.segment"
          />
        </n-collapse-item>
        <n-collapse-item :title="`FPU TagWord: ${DataFormatter.formatNumberHex(props.registers.fpuTagWord, 4)}`">
          <n-data-table
            :bordered="false"
            :columns="[
              { title: '名称', key: 'name1' },
              { title: '值', key: 'value1' },
              { title: '位置', key: 'range1' },
              { title: '名称', key: 'name2' },
              { title: '值', key: 'value2' },
              { title: '位置', key: 'range2' },
            ]"
            :data="registerData.fpuTagWord"
          />
        </n-collapse-item>
        <n-collapse-item :title="`FPU 状态寄存器: ${DataFormatter.formatNumberHex(props.registers.fpuStatusWord, 4)}`">
          <n-data-table
            :bordered="false"
            :columns="[
              { title: '名称', key: 'name1' },
              { title: '值', key: 'value1' },
              { title: '位置', key: 'range1' },
              { title: '名称', key: 'name2' },
              { title: '值', key: 'value2' },
              { title: '位置', key: 'range2' },
            ]"
            :data="registerData.fpuStatusWord"
          />
        </n-collapse-item>
        <n-collapse-item :title="`FPU 控制寄存器: ${DataFormatter.formatNumberHex(props.registers.fpuControlWord, 4)}`">
          <n-data-table
            :bordered="false"
            :columns="[
              { title: '名称', key: 'name1' },
              { title: '值', key: 'value1' },
              { title: '位置', key: 'range1' },
              { title: '名称', key: 'name2' },
              { title: '值', key: 'value2' },
              { title: '位置', key: 'range2' },
            ]"
            :data="registerData.fpuControlWord"
          />
        </n-collapse-item>
        <n-collapse-item title="X87 浮点寄存器">
          <n-data-table
            :bordered="false"
            :columns="[
              { title: '名称', key: 'name' },
              { title: '值', key: 'value' },
              { title: '数据 (63 - 0)', key: 'content' },
            ]"
            :data="registerData.fpu"
          />
        </n-collapse-item>
        <n-collapse-item :title="`MxCsr 标志寄存器: ${DataFormatter.formatNumberHex(props.registers.mxCsr)}`">
          <n-data-table
            :bordered="false"
            :columns="[
              { title: '名称', key: 'name1' },
              { title: '值', key: 'value1' },
              { title: '位置', key: 'range1' },
              { title: '名称', key: 'name2' },
              { title: '值', key: 'value2' },
              { title: '位置', key: 'range2' },
            ]"
            :data="registerData.mxCsr"
          />
        </n-collapse-item>
        <n-collapse-item title="SIMD 寄存器">
          <n-data-table
            :bordered="false"
            :columns="[
              { title: '名称', key: 'name' },
              { title: '值', key: 'value' },
            ]"
            :data="registerData.simd"
          />
        </n-collapse-item>
        <n-collapse-item title="调试寄存器">
          <n-data-table
            :bordered="false"
            :columns="[
              { title: '名称', key: 'name1' },
              { title: '值', key: 'value1' },
              { title: '名称', key: 'name2' },
              { title: '值', key: 'value2' },
            ]"
            :data="registerData.debug"
          />
        </n-collapse-item>
      </n-collapse>
    </n-card>
  </n-scrollbar>
</template>
