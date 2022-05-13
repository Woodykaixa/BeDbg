<script lang="ts" setup>
import { computed } from 'vue';
import { DataFormatter } from '@/util/formatter';
import { useDebugData } from '@/hooks/useDebugData';
import { NCode } from 'naive-ui';
const debugData = useDebugData();

const props = defineProps({
  address: {
    type: Number,
    required: true,
  },
  text: {
    type: String,
    required: true,
  },
});

const bpAction = computed(() =>
  debugData.breakPoints.has(props.address) ? debugData.removeBreakpoint : debugData.setBreakpoint
);

const highlight = computed(() => debugData.mainProcess.mainThread.registers.rip === props.address);
</script>
<template>
  <div :class="highlight ? 'highlight' : ''" class="dis-asm-instr">
    <div class="bp-box" @click="bpAction(props.address)">
      <div v-if="debugData.breakPoints.has(props.address)" class="bp bp-set"></div>
      <div v-else class="bp"></div>
    </div>
    <div class="address">{{ DataFormatter.formatNumberHex(props.address) }}</div>
    <n-code :code="props.text" language="x86asm" />
  </div>
</template>

<style scoped>
.dis-asm-instr {
  width: 100%;
  display: flex;
}

.dis-asm-instr .address {
  min-width: 150px;
  max-width: 200px;
  text-align: center;
}

.bp-box {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 16px;
  margin-left: 4px;
  cursor: pointer;
}

.bp {
  width: 10px;
  height: 10px;
  opacity: 0;
  border-radius: 50%;
  background-color: red;
}

.bp-box:hover .bp {
  opacity: 0.5;
}

.bp-set {
  opacity: 0.8;
}

.bp-box:hover .bp-set {
  opacity: 0.9;
}

.highlight {
  background-color: rgba(253, 248, 107, 0.3);
}
</style>
