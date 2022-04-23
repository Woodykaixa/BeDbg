<script lang="ts" setup>
import DisassmblyView from './DisassmblyView.vue';
import DebugControlPanel from './DebugControlPanel.vue';
import RegisterView from './RegisterView.vue';
import { useDebugData } from '@/hooks/useDebugData';
import { DefaultEmptyRegisters } from '@/dto/thread';
import { ref, onMounted } from 'vue';
import { Api } from '@/api';
import { useLoadingStates } from '@/hooks/useLoadingStates';

const debugData = useDebugData();
const loadingStates = useLoadingStates();
const registers = ref(DefaultEmptyRegisters);

onMounted(async () => {
  const { ok, data } = await Api.DebuggingProcess.getRegisters(
    debugData.mainProcess.id,
    debugData.mainProcess.mainThread.id
  );
  if (ok) {
    registers.value = data;
  } else {
    console.error('fetch register failed', data);
  }
});
</script>

<template>
  <div class="debug-view">
    <debug-control-panel class="debug-view_debug-control" />
    <div class="debug-view_content">
      <disassmbly-view class="debug-view_dis-asm-view" />
      <register-view class="debug-view_register-view" :registers="registers" />
    </div>
  </div>
</template>

<style scoped>
.debug-view {
  width: 100%;
  height: 100vh;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  box-sizing: border-box;
}

.debug-view_debug-control {
  height: 5vh;
}

.debug-view_content {
  box-sizing: border-box;
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  grid-template-rows: repeat(2, 1fr);
  flex-direction: column;
  justify-content: center;
  width: 80%;
  min-width: 80vw;
  height: 95vh;
  gap: 8px;
  padding: 8px;
}

.debug-view_dis-asm-view {
  box-sizing: border-box;
  grid-column: 1 / 2;
  grid-row: 1 / 3;
  width: 100%;
  height: 100%;
}

.debug-view_register-view {
  grid-column: 2 / 3;
  grid-row: 1 / 2;
  width: 100%;
  height: 100%;
}
</style>
