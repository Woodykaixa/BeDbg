<script lang="ts" setup>
import { NConfigProvider, darkTheme, NGlobalStyle, NThemeEditor } from 'naive-ui';
import type { GlobalThemeOverrides } from 'naive-ui';
</script>

<template>
  <n-config-provider :theme="darkTheme">
    <n-theme-editor>
      <router-view></router-view>
      <n-global-style />
    </n-theme-editor>
  </n-config-provider>
</template>
