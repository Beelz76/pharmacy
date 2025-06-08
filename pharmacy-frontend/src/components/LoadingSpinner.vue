<template>
  <div
    :class="[
      'animate-spin rounded-full border-4 border-t-transparent',
      spinnerSize,
      colorClass,
      customClass,
    ]"
    role="status"
    aria-label="Загрузка..."
  ></div>
</template>

<script setup>
import { computed } from "vue";

const props = defineProps({
  size: {
    type: String,
    default: "md", // sm | md | lg
  },
  color: {
    type: String,
    default: "primary",
  },
  class: {
    type: String,
    default: "",
  },
});

const spinnerSize = computed(() => {
  switch (props.size) {
    case "sm":
      return "h-4 w-4";
    case "lg":
      return "h-10 w-10";
    default:
      return "h-8 w-8";
  }
});

const colorClass = computed(() => {
  return `border-${props.color}-600`;
});

const customClass = computed(() => props.class);
</script>
