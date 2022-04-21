<script lang="ts" setup>
import { DataFormatter } from '@/util/formatter';
import { useRegisters } from '@/hooks/useRegisters';
import {
  SimdRegisters,
  FpuStackRegisters,
  GeneralPurposeRegisters,
  Registers,
  Number128,
  DebugRegisters,
  SegmentRegisters,
} from '@/dto/thread';
import { useDebuggerEventSource } from '@/hooks/useDebuggerEvent';
import { onMounted, PropType, watch, reactive, onUpdated, h } from 'vue';
import { DefaultEmptyRegisters } from '@/dto/thread';
import { NDataTable, DataTableColumn } from 'naive-ui';

const props = defineProps({
  registers: {
    type: Object as PropType<Registers>,
    required: true,
  },
});

type RegData = Array<{ name: string; value: string }>;

const registerData = reactive({
  general: [] as RegData,
  fpu: [] as RegData,
  simd: [] as RegData,
  segment: [] as RegData,
  debug: [] as RegData,
});

Object.entries(props.registers).forEach(([name, value]) => {
  if (GeneralPurposeRegisters.includes(name as any)) {
    registerData.general.push({ name: name.toUpperCase(), value: DataFormatter.formatNumberHex(value as number) });
  } else if (SimdRegisters.includes(name as any)) {
    const val = value as Number128;
    registerData.simd.push({ name: name.toUpperCase(), value: DataFormatter.formatNumberHex([val.high, val.low]) });
  } else if (FpuStackRegisters.includes(name as any)) {
    const val = value as Number128;
    registerData.fpu.push({ name: name.toUpperCase(), value: DataFormatter.formatNumberHex([val.high, val.low]) });
  } else if (DebugRegisters.includes(name as any)) {
    registerData.debug.push({ name: name.toUpperCase(), value: DataFormatter.formatNumberHex(value as number) });
  } else if (SegmentRegisters.includes(name as any)) {
    registerData.segment.push({
      name: name.toUpperCase().slice(3),
      value: DataFormatter.formatNumberHex(value as number),
    });
  }
});

// const [registers] = useRegisters();
// const reg = Object.entries(props.registers);
// console.log('reg arr', reg);
const columns: DataTableColumn[] = [
  { title: '寄存器', key: 'name' },
  { title: '值', key: 'value' },
];
</script>

<template>
  <n-data-table
    :bordered="false"
    :columns="[
      { title: '通用寄存器', key: 'name' },
      { title: '值', key: 'value' },
    ]"
    :data="registerData.general"
  />
  <n-data-table
    :bordered="false"
    :columns="[
      { title: '段寄存器', key: 'name' },
      { title: '值', key: 'value' },
    ]"
    :data="registerData.segment"
  />
  <n-data-table
    :bordered="false"
    :columns="[
      { title: 'SIMD 寄存器', key: 'name' },
      { title: '值', key: 'value' },
    ]"
    :data="registerData.simd"
  />
  <n-data-table
    :bordered="false"
    :columns="[
      { title: 'X87 浮点寄存器', key: 'name' },
      { title: '值', key: 'value' },
    ]"
    :data="registerData.fpu"
  />
  <n-data-table
    :bordered="false"
    :columns="[
      { title: '调试寄存器', key: 'name' },
      { title: '值', key: 'value' },
    ]"
    :data="registerData.debug"
  />
  <!-- <div class="register-box" :style="{ display: 'flex', 'flex-wrap': 'wrap' }">
    <code class="name">通用寄存器</code>
    <code class="value"></code>
    <div v-for="(value, name) in registerData.general" class="register">
      <code class="name">{{ name }}</code>
      <code class="value">{{ DataFormatter.formatNumberHex(value) }}</code>
    </div>
  </div>
  <div class="register-box" :style="{ display: 'flex', 'flex-wrap': 'wrap' }">
    <code class="name">段寄存器</code>
    <code class="value"></code>
    <div v-for="(value, name) in registerData.segment" class="register">
      <code class="name">{{ name }}</code>
      <code class="value">{{ DataFormatter.formatNumberHex(value, 4) }}</code>
    </div>
  </div>
  <div class="register-box" :style="{ display: 'flex', 'flex-wrap': 'wrap' }">
    <code class="name">X87 浮点数寄存器</code>
    <code class="value"></code>
    <div v-for="(value, name) in registerData.fpu" class="register">
      <code class="name">{{ name }}</code>
      <code class="value">{{ DataFormatter.formatNumberHex([value.high, value.low]) }}</code>
    </div>
  </div>
  <div class="register-box" :style="{ display: 'flex', 'flex-wrap': 'wrap' }">
    <code class="name">rax</code>
    <code class="value">{{ DataFormatter.formatNumberHex(props.registers.rax) }}</code>
  </div> -->
</template>

<style scoped>
.register-box {
  margin-left: 4px;
}

.register-box .register {
  box-sizing: border-box;
  padding: 4px;
  display: flex;
}

.register-box .register .name {
  width: 30px;
  margin: 0 4px;
}

.register-box .register .value {
}

.n-data-table {
  margin-top: 4px;
}
</style>
