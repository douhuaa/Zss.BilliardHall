<template>
  <span
    class="svg-icon"
    :class="[`svg-icon--${name}`, { 'is-spin': spin }]"
    :style="computedStyle"
    role="img"
    :aria-label="ariaLabel || name"
  >
    <!-- 优先使用 sprite -->
    <svg v-if="useSprite" :width="size" :height="size" :viewBox="viewBoxComputed" fill="none" aria-hidden="false">
      <use :href="`#icon-${name}`" :fill="color || 'currentColor'" />
    </svg>
    <!-- fallback: 单文件 svg 通过动态 import (可选) -->
    <component
      v-else-if="svgComponent"
      :is="svgComponent"
      :style="{ width: sizePx, height: sizePx, display: 'inline-block' }"
      aria-hidden="false"
    />
    <slot v-else />
  </span>
</template>

<script setup>
import { computed, ref, onMounted } from 'vue'

const props = defineProps({
  name: { type: String, required: true },            // 对应 sprite id 去掉 icon- 前缀
  size: { type: [Number, String], default: 24 },     // 尺寸（px 或 带单位）
  color: { type: String, default: '' },              // 自定义颜色（未设置则继承 currentColor）
  spin: { type: Boolean, default: false },           // 是否旋转动画
  ariaLabel: { type: String, default: '' },
  fallback: { type: Boolean, default: true },        // 是否尝试 fallback 到单文件 svg
  viewBox: { type: String, default: '' }             // 自定义 viewBox（不传则依赖 sprite 本身）
})

const svgComponent = ref(null)

const sizePx = computed(() => (typeof props.size === 'number' ? `${props.size}px` : props.size))

const computedStyle = computed(() => ({
  color: props.color || undefined,
  width: sizePx.value,
  height: sizePx.value,
  lineHeight: 0,
  display: 'inline-flex',
  alignItems: 'center',
  justifyContent: 'center'
}))

const useSprite = computed(() => !!props.name)

const viewBoxComputed = computed(() => props.viewBox || undefined)

async function tryLoadFallback() {
  if (!props.fallback || !props.name) return
  try {
    // 假定存在同名 svg 文件：/src/static/<name>.svg
    const mod = await import(/* @vite-ignore */ `../static/${props.name}.svg?component`)
    svgComponent.value = mod.default
  } catch (e) {
    // 忽略未找到
  }
}

onMounted(() => {
  if (!useSprite.value) {
    tryLoadFallback()
  }
})
</script>

<style scoped>
.svg-icon { cursor: default; }
.svg-icon.is-spin svg { animation: svg-icon-rotate 1s linear infinite; }
@keyframes svg-icon-rotate { from { transform: rotate(0deg);} to { transform: rotate(360deg);} }
</style>
