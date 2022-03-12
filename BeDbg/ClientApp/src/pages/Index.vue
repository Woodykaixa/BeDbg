<script setup lang="ts">
import { NTabs, NTabPane } from 'naive-ui';
import ProcessSelector from '@/components/ProcessSelector.vue';
import FileSelector from '@/components/FileSelector.vue';
</script>

<template>
  <div class="page">
    <div style="display: flex; flex-direction: column; width: 60%">
      <h1 class="title">BeDbg</h1>
      <n-tabs default-value="create" type="line">
        <n-tab-pane name="create" tab="调试文件">
          <file-selector></file-selector>
        </n-tab-pane>
        <n-tab-pane name="attach" tab="附加到进程">
          <process-selector></process-selector>
        </n-tab-pane>
      </n-tabs>
    </div>
  </div>
</template>

<style scoped>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}

.page {
  width: 100vw;
  height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
}

.title {
  font-size: 3rem;
  text-align: center;
}
</style>
