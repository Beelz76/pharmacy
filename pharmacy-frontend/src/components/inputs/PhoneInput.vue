<template>
  <component
    :is="wrapWithFormItem ? 'el-form-item' : 'div'"
    v-bind="wrapWithFormItem ? { label, prop: name, rules: validationRules } : {}"
    class="w-full"
  >
    <div class="w-full">
      <el-input
        v-model="inputValue"
        v-mask="'+7 (###) ###-##-##'"
        :placeholder="placeholder"
        :readonly="readonly"
        :disabled="disabled"
        maxlength="18"
        class="w-full !h-12 !text-sm !rounded-md"
      />
      <div
        v-if="!required && wrapWithFormItem && !inputValue.trim()"
        class="text-xs text-gray-400 ml-1"
      >
        Необязательное поле
      </div>
    </div>
  </component>
</template>


<script setup>
import { ref, watch } from 'vue'

const props = defineProps({
  modelValue: String,
  label: String,
  name: { type: String, default: 'phone' },
  placeholder: { type: String, default: '+7 (___) ___-__-__' },
  required: { type: Boolean, default: false },
  readonly: Boolean,
  disabled: Boolean,
  wrapWithFormItem: { type: Boolean, default: true },
  digitsOnly: { type: Boolean, default: false },
  size: { type: String, default: 'default' }
})

const emit = defineEmits(['update:modelValue'])

const inputValue = ref('')

// Просто подставим value, без форматирования
watch(
  () => props.modelValue,
  (newVal) => {
    inputValue.value = typeof newVal === 'string' ? newVal : ''
  },
  { immediate: true }
)

// Отправляем только цифры наружу
watch(inputValue, (val) => {
  if (props.digitsOnly) {
    emit('update:modelValue', val.replace(/\D/g, ''))
  } else {
    emit('update:modelValue', val)
  }
})

// Валидация
const validationRules = [
  {
    validator(_, val, cb) {
      const digits = (val || '').replace(/\D/g, '')
      if (!digits && !props.required) return cb()
      if (digits.length !== 11 || !digits.startsWith('7')) {
        return cb(new Error('Введите корректный номер'))
      }
      cb()
    },
    trigger: ['blur']
  }
]


</script>
