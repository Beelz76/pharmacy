import { ref } from "vue";

export default function useResendTimer(initial = 60) {
  const resendTimer = ref(initial);
  const canResend = ref(false);
  let interval = null;

  function clear() {
    if (interval) {
      clearInterval(interval);
      interval = null;
    }
  }

  function start() {
    clear();
    resendTimer.value = initial;
    canResend.value = false;
    interval = setInterval(() => {
      resendTimer.value--;
      if (resendTimer.value <= 0) {
        clear();
        canResend.value = true;
      }
    }, 1000);
  }

  return { resendTimer, canResend, start, clear };
}
