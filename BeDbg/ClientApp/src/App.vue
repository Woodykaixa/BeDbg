<script lang="ts" setup>
import { NConfigProvider, darkTheme, NGlobalStyle, NIcon, NButton, NPopover, NNotificationProvider } from 'naive-ui';
import { SettingOutlined } from '@vicons/antd';
import SettingPanel from './components/SettingPanel.vue';
</script>

<template>
  <n-config-provider :theme="darkTheme">
    <n-notification-provider>
      <router-view></router-view>
      <n-popover trigger="click" :style="{ width: '400px' }">
        <template #trigger>
          <n-button class="setting-button" text>
            <n-icon size="30">
              <setting-outlined />
            </n-icon>
          </n-button>
        </template>
        <setting-panel />
      </n-popover>
    </n-notification-provider>
    <n-global-style />
  </n-config-provider>
</template>

<style scoped>
.setting-button {
  position: absolute;
  width: 50px;
  height: 50px;
  right: 40px;
  bottom: 40px;
  border-radius: 50%;
  background-color: #212121;
}

.setting-button:hover,
.setting-button:focus {
  background-color: #323232;
}
</style>
