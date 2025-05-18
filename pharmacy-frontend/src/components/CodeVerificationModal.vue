<template>
  <el-dialog
    :model-value="visible"
    @update:model-value="$emit('update:visible', $event)"
    width="420px"
    class="rounded-xl"
    :modal-append-to-body="false"
    :lock-scroll="false"
    align-center
    :append-to-body="false"
    teleported="false"
  >
    <template #header>
      <div class="flex items-center justify-center relative h-10">
        <div class="text-xl font-semibold">Подтверждение</div>
      </div>
    </template>

    <div class="space-y-4 mt-2">
      <div class="flex justify-center gap-2 mb-2">
        <input
          v-for="(digit, index) in codeDigits"
          :key="index"
          :ref="el => codeInputs[index] = el"
          maxlength="1"
          inputmode="numeric"
          type="text"
          class="w-12 h-12 border rounded-md text-center text-xl"
          v-model="codeDigits[index]"
          @input="onCodeInput(index)"
          @keydown.backspace="onBackspace(index)"
          @paste.prevent="onPaste($event)"
          @keyup.enter="onEnter"
        />
      </div>

      <el-button
        type="primary"
        class="w-full mt-2 !h-11"
        :disabled="verificationCode.length !== 6"
        :loading="loading"
        @click="confirmCode"
      >
        Подтвердить
      </el-button>

      <div class="text-center text-sm text-gray-500 mt-3">
        <template v-if="!canResend">
          Отправить код повторно через {{ resendTimer }} сек
        </template>
        <template v-else>
          <el-button
            class="!h-8 !px-3 !text-sm"
            type="primary"
            text
            size="small"
            :loading="resendLoading"
            :disabled="resendLoading"
            @click="resendCode"
          >
            Отправить код снова
          </el-button>
        </template>
      </div>
    </div>
  </el-dialog>
</template>

<script setup>
import { ref, watchEffect, nextTick, onUnmounted } from 'vue'
import { ElMessage } from 'element-plus'
import VerificationService from '/src/services/VerificationService.js'

const props = defineProps({
  visible: Boolean,
  email: String,
  purpose: String
})

const emit = defineEmits(['close', 'success', 'update:visible'])

const resendLoading = ref(false)
const loading = ref(false)
const verificationCode = ref('')
const resendTimer = ref(60)
const resendInterval = ref(null)
const canResend = ref(false)
const errorMessage = ref('')

const codeInputs = ref([])
const codeDigits = ref(['', '', '', '', '', ''])

watchEffect(() => {
  if (props.visible) {
    codeDigits.value = ['', '', '', '', '', '']
    verificationCode.value = ''
    errorMessage.value = ''
    canResend.value = false
    resendTimer.value = 60
    nextTick(() => codeInputs.value[0]?.focus())

    setTimeout(() => {
      startResendTimer()
    }, 50)
  } else {
    clearInterval(resendInterval.value)
    resendInterval.value = null
  }
})

function startResendTimer() {
  clearInterval(resendInterval.value)
  resendInterval.value = null
  resendTimer.value = 60
  canResend.value = false

  resendInterval.value = setInterval(() => {
    resendTimer.value--
    if (resendTimer.value <= 0) {
      clearInterval(resendInterval.value)
      resendInterval.value = null
      canResend.value = true
    }
  }, 1000)
}

async function resendCode() {
  if (!canResend.value || resendLoading.value) return
  resendLoading.value = true
  try {
    await VerificationService.sendCode(props.email, props.purpose)
    ElMessage.success('Код отправлен на почту')
    startResendTimer()
    codeDigits.value = ['', '', '', '', '', '']
    verificationCode.value = ''
    nextTick(() => codeInputs.value[0]?.focus())
  } catch (err) {
    ElMessage.error('Ошибка при отправке кода')
  } finally {
    resendLoading.value = false
  }
}


async function confirmCode() {
  if (loading.value) return
  loading.value = true
  try {
    const result = await VerificationService.confirmCode(props.email, verificationCode.value, props.purpose)
    ElMessage.success('Код подтверждён')

    emit('success', result)
    emit('close')
  } catch (err) {
    errorMessage.value = err.message || 'Ошибка подтверждения'
  } finally {
    loading.value = false
  }
}


function onCodeInput(index) {
  const val = codeDigits.value[index]
  if (!/\d/.test(val)) {
    codeDigits.value[index] = ''
    return
  }
  if (index < 5) nextTick(() => codeInputs.value[index + 1]?.focus())
  verificationCode.value = codeDigits.value.join('')
  if (verificationCode.value.length === 6) confirmCode()
}

function onBackspace(index) {
  if (codeDigits.value[index]) {
    codeDigits.value[index] = ''
  } else if (index > 0) {
    nextTick(() => codeInputs.value[index - 1]?.focus())
  }
}

function onPaste(event) {
  const pasted = event.clipboardData.getData('text').replace(/\D/g, '').slice(0, 6)
  for (let i = 0; i < 6; i++) {
    codeDigits.value[i] = pasted[i] || ''
  }
  verificationCode.value = codeDigits.value.join('')
  nextTick(() => codeInputs.value[Math.min(pasted.length, 5)]?.focus())
  if (verificationCode.value.length === 6) confirmCode()
}

function onEnter() {
  if (verificationCode.value.length === 6) confirmCode()
}

onUnmounted(() => {
  clearInterval(resendInterval.value)
  resendInterval.value = null
})
</script>
