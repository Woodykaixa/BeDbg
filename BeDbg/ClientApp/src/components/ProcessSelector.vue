<script setup lang="ts">
import { ref, effect, onMounted } from 'vue';
import { NSpace, NList, NListItem, NThing, NScrollbar, NEmpty, NSpin, NTag, NButton } from 'naive-ui';
import { ProcessModel } from '../dto/process';
import { Api } from '@/api';

const loading = ref(true);
const processes = ref([] as Array<ProcessModel>);
const requestProcessList = () => {
  effect(async () => {
    const processList = await Api.getProcessList();
    if (processList) {
      loading.value = false;
      processes.value = processList;
    }
  });
};

onMounted(requestProcessList);

const selectedPid = ref(0);
const selectProcess = (pid: number) => {
  selectedPid.value = pid;
};
</script>

<template>
  <n-spin :show="loading">
    <n-list class="panel">
      <template #header>
        <div style="display: flex; justify-content: space-between">
          <div>当前进程</div>
          <n-button :disabled="selectedPid === 0">附加</n-button>
        </div>
      </template>
      <n-scrollbar v-if="processes.length !== 0" style="height: 50vh; padding: 8px; background-color: rgb(26, 26, 26)">
        <n-list-item
          v-for="process in processes"
          :key="process.id"
          class="process-item"
          @click="selectProcess(process.id)"
        >
          <n-thing>
            <template #header>
              <n-space>
                <div>进程名称: {{ process.name }}</div>
                <n-tag v-if="process.wow64" type="success" size="small">WOW64</n-tag>
              </n-space>
            </template>
            <template #description>
              <div>进程ID: {{ process.id }}</div>
              <div>进程标题: {{ process.title }}</div>
              <div>启动命令: {{ process.command }}</div>
            </template>
          </n-thing>
        </n-list-item>
      </n-scrollbar>
      <n-empty v-else title="没有获取到进程信息" class="panel-empty">
        <template #extra> 请尝试使用管理员模式运行 BeDbg </template>
      </n-empty>
    </n-list>
  </n-spin>
</template>

<style scoped>
.panel {
  width: 100%;
  height: 60vh;
  position: relative;
}

.panel-empty {
  width: 100%;
  height: 100%;
  position: absolute;
  left: 50%;
  top: 50%;
  transform: translate(-50%, -50%);
  justify-content: center;
}
.process-item {
  cursor: pointer;
  padding: 4px;
}

.process-item:hover {
  background-color: rgba(99, 226, 183, 0.6);
  transition-timing-function: linear;
  transition: 0.2s;
}
</style>
