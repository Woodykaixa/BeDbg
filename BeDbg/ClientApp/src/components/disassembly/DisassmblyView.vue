<script setup lang="ts">
import { NList, NLi, NScrollbar, NCode, NSpin, NButton, ScrollbarInst } from 'naive-ui';
import { DataFormatter } from '@/util/formatter';
import { useDebugData } from '@/hooks/useDebugData';
import { useLoadingStates } from '@/hooks/useLoadingStates';
import { computed, h, ref } from 'vue';
import { VueVirtualScroller } from '@wefly/vue-virtual-scroller';
import DisplayingInstruction from './DisplayingInstruction.vue';
import { useDebuggerEventSource } from '@/hooks/useDebuggerEvent';
import type { DebuggerEventListener } from '@/util/debuggerEventSource';

import '@wefly/vue-virtual-scroller/dist/style.css';
import './DisassemblyView.css';

const debugData = useDebugData();
const loadingStates = useLoadingStates();
const asmInstructionList = ref<ScrollbarInst>();

const debuggerEvent = useDebuggerEventSource();

const onBreakpoint: DebuggerEventListener<'breakpoint' | 'singleStep'> = async data => {
  await debugData.updateRegisters(data.process, data.thread);
  console.log('instrList', asmInstructionList.value);
  const index = debugData.mainProcess.mainThread.instructions.findIndex(i => i.address === data.exceptionAddress);
  console.log('index', index);
  asmInstructionList.value?.scrollTo({
    top: index * 22.4,
  });
};

debuggerEvent.addEventListener('breakpoint', onBreakpoint);
debuggerEvent.addEventListener('singleStep', onBreakpoint);
</script>

<template>
  <div class="debug-container">
    <n-list class="dis-asm-box" bordered>
      <template #header> 反汇编 </template>
      <n-scrollbar ref="asmInstructionList">
        <n-spin :show="loadingStates.disassemblyState === 'loading'">
          <vue-virtual-scroller
            :list="debugData.mainProcess.mainThread.instructions"
            :activeLen="debugData.mainProcess.mainThread.instructions.length"
          >
            <template v-slot:default="{ item }">
              <displaying-instruction :address="item.address" :text="item.text" />
            </template>
          </vue-virtual-scroller>
        </n-spin>
      </n-scrollbar>
    </n-list>
  </div>
</template>

<style scoped>
.debug-container {
  box-sizing: border-box;
  display: flex;
  margin: auto;
}

.dis-asm-box {
  width: 100%;
  display: flex;
  flex-direction: column;
  font-family: Consolas;
  margin-block-start: 0;
  margin-block-end: 0;
}

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
}

.bp {
  width: 10px;
  height: 10px;
  opacity: 0;
  border-radius: 50%;
  cursor: pointer;
  background-color: red;
}

.bp:hover {
  opacity: 0.5;
}

.bp-set {
  opacity: 0.8;
}

.bp-set:hover {
  opacity: 0.9;
}

.breakpoint-hit {
  background-color: rgba(253, 248, 107, 0.5);
}
</style>
