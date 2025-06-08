import api from "../utils/axios";

export default {
  async sendCode(email, purpose) {
    return await api.post("/verifications/send-code", { email, purpose });
  },

  async confirmCode(email, code, purpose) {
    return await api.post("/verifications/confirm-code", {
      email,
      code,
      purpose,
    });
  },
};
