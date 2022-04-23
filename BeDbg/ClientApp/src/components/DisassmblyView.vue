<script setup lang="ts">
import { effect, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useNotification, NList, NLi, NScrollbar, NCode, NSpin, NLayoutSider, NLayoutContent } from 'naive-ui';
import { Api } from '@/api';
import { DebuggerEventSource } from '@/util/debuggerEventSource';
import { DataFormatter } from '@/util/formatter';
import DebugViewSider from '@/components/DebugViewSider.vue';
import { provideDebuggerEventSource } from '@/hooks/useDebuggerEvent';
import { useDebugData, WinProcess, WinThread } from '@/hooks/useDebugData';
import { useLoadingStates } from '@/hooks/useLoadingStates';

const debugData = useDebugData();
const loadingStates = useLoadingStates();
</script>

<template>
  <div class="debug-container">
    <n-list class="dis-asm-box" bordered>
      <template #header> 反汇编 </template>
      <n-spin :show="loadingStates.disassemblyState === 'loading'">
        <n-scrollbar>
          <n-li v-for="i in debugData.mainProcess.mainThread.instructions" class="dis-asm-instr">
            <div class="address">{{ DataFormatter.formatNumberHex(i.address) }}</div>
            <n-code :code="i.text" language="x86asm" />
          </n-li>
        </n-scrollbar>
      </n-spin>
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
</style>
