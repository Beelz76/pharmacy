import { ref, nextTick } from "vue";

export default function useVerificationCodeInput(length = 6) {
  const codeInputs = ref([]);
  const codeDigits = ref(Array(length).fill(""));
  const verificationCode = ref("");

  function updateCode() {
    verificationCode.value = codeDigits.value.join("");
  }

  function onCodeInput(index) {
    const val = codeDigits.value[index];
    if (!/\d/.test(val)) {
      codeDigits.value[index] = "";
      return;
    }
    if (index < length - 1) {
      nextTick(() => codeInputs.value[index + 1]?.focus());
    }
    updateCode();
  }

  function onBackspace(index) {
    if (!codeDigits.value[index] && index > 0) {
      nextTick(() => codeInputs.value[index - 1]?.focus());
    }
  }

  function onPaste(event) {
    const pasted = event.clipboardData
      .getData("text")
      .replace(/\D/g, "")
      .slice(0, length);
    for (let i = 0; i < length; i++) {
      codeDigits.value[i] = pasted[i] || "";
    }
    updateCode();
    nextTick(() =>
      codeInputs.value[Math.min(pasted.length, length - 1)]?.focus()
    );
  }

  function reset() {
    codeDigits.value = Array(length).fill("");
    verificationCode.value = "";
    nextTick(() => codeInputs.value[0]?.focus());
  }

  return {
    codeInputs,
    codeDigits,
    verificationCode,
    onCodeInput,
    onBackspace,
    onPaste,
    reset,
  };
}
