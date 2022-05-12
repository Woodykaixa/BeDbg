<script setup lang="ts">
import { NList, NLi, NScrollbar, NCode, NSpin, NButton } from 'naive-ui';
import { DataFormatter } from '@/util/formatter';
import { useDebugData } from '@/hooks/useDebugData';
import { useLoadingStates } from '@/hooks/useLoadingStates';
import { computed, h, ref } from 'vue';
import { VueVirtualScroller } from '@wefly/vue-virtual-scroller';
import '@wefly/vue-virtual-scroller/dist/style.css';

const debugData = useDebugData();
const loadingStates = useLoadingStates();
const bpAddress = ref(new Set<number>());

const setBreakpoint = async (address: number) => {
  console.log('setBreakpoint', address);
  bpAddress.value.add(address);
};

const removeBreakpoint = async (address: number) => {
  bpAddress.value.delete(address);
};
</script>

<template>
  <div class="debug-container">
    <n-list class="dis-asm-box" bordered>
      <template #header> 反汇编 </template>
      <n-scrollbar>
        <n-spin :show="loadingStates.disassemblyState === 'loading'">
          <vue-virtual-scroller
            :list="debugData.mainProcess.mainThread.instructions"
            :activeLen="debugData.mainProcess.mainThread.instructions.length"
          >
            <template v-slot:default="{ item }">
              <n-li class="dis-asm-instr">
                <div class="bp-box">
                  <div
                    v-if="bpAddress.has(item.address)"
                    class="bp bp-set"
                    @click="removeBreakpoint(item.address)"
                  ></div>
                  <div v-else class="bp" @click="setBreakpoint(item.address)"></div>
                </div>
                <div class="address">{{ DataFormatter.formatNumberHex(item.address) }}</div>
                <n-code :code="item.text" language="x86asm" />
              </n-li>
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

.virtual-scroll {
  overflow: hidden;
}
</style>
