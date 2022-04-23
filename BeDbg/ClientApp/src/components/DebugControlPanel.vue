<script lang="ts" setup>
import { NCard, NElement, NSpace, NButton, NIcon } from 'naive-ui';
import {
  StopOutlined,
  PauseOutlined,
  ArrowDownOutlined,
  ArrowUpOutlined,
  ArrowRightOutlined,
  CloseOutlined,
  ExpandOutlined,
} from '@vicons/antd';
</script>

<template>
  <n-element>
    <div justify="center" class="debug-control" align="center">
      <n-button quaternary type="success" title="暂停程序">
        <template #icon>
          <n-icon size="20">
            <pause-outlined />
          </n-icon>
        </template>
      </n-button>
      <n-button quaternary type="success" title="单步执行 (Step In)">
        <template #icon>
          <n-icon size="20">
            <arrow-down-outlined />
          </n-icon>
        </template>
      </n-button>
      <n-button quaternary type="success" title="单步执行 (Step Over)">
        <template #icon>
          <n-icon size="20">
            <arrow-right-outlined />
          </n-icon>
        </template>
      </n-button>
      <n-button quaternary type="success" title="退出函数 (Step Out)">
        <template #icon>
          <n-icon size="20">
            <arrow-up-outlined />
          </n-icon>
        </template>
      </n-button>
      <n-button quaternary type="error" title="结束调试">
        <template #icon>
          <n-icon size="20">
            <close-outlined />
          </n-icon>
        </template>
      </n-button>
      <n-button quaternary title="其他功能">
        <template #icon>
          <n-icon size="20">
            <expand-outlined />
          </n-icon>
        </template>
      </n-button>
    </div>
  </n-element>
</template>

<style scoped>
.debug-control {
  background: var(--modal-color);
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 8px;
}
</style>
