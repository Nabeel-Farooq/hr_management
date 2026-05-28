<template>
  <q-page class="full-height full-width flex flex-center">
    <div class="container">
      <div class="avatar flex flex-center q-pt-md column">
        <q-avatar size="200px">
          <img :src="getInformation?.avatar?.original" />
        </q-avatar>
      </div>

      <!-- Old Password -->
      <div class="old-password q-mt-lg q-px-md">
        <q-input
          ref="oldRef"
          v-model.trim="oldPassword"
          standout
          lazy-rules
          maxlength="500"
          hide-bottom-space
          :type="showPassword[0] ? 'text' : 'password'"
          :label="$t('oldPassword')"
          :label-color="labelColorFocus[0]"
          :rules="oldPasswordRules"
          @focus="labelColorFocus[0] = 'white'"
          @blur="labelColorFocus[0] = ''"
        >
          <template #prepend>
            <q-icon name="fas fa-lock" />
          </template>

          <template #append>
            <q-icon
              class="cursor-pointer"
              :name="showPassword[0] ? 'visibility_off' : 'visibility'"
              @click="togglePassword(0)"
            />
          </template>
        </q-input>
      </div>

      <!-- New Password -->
      <div class="new-password q-mt-sm q-px-md">
        <q-input
          ref="newRef"
          v-model.trim="newPassword"
          standout
          lazy-rules
          maxlength="100"
          hide-bottom-space
          :type="showPassword[1] ? 'text' : 'password'"
          :label="$t('newPassword')"
          :label-color="labelColorFocus[1]"
          :rules="newPasswordRules"
          @focus="labelColorFocus[1] = 'white'"
          @blur="labelColorFocus[1] = ''"
        >
          <template #prepend>
            <q-icon name="password" />
          </template>

          <template #append>
            <q-icon
              class="cursor-pointer"
              :name="showPassword[1] ? 'visibility_off' : 'visibility'"
              @click="togglePassword(1)"
            />
          </template>
        </q-input>
      </div>

      <!-- Confirm Password -->
      <div class="confirm-password q-mt-sm q-px-md">
        <q-input
          ref="confirmRef"
          v-model.trim="confirmPassword"
          standout
          lazy-rules
          maxlength="100"
          hide-bottom-space
          :type="showPassword[1] ? 'text' : 'password'"
          :label="$t('confirmPwd')"
          :label-color="labelColorFocus[2]"
          :rules="confirmPasswordRules"
          @focus="labelColorFocus[2] = 'white'"
          @blur="labelColorFocus[2] = ''"
        >
          <template #prepend>
            <q-icon name="password" />
          </template>

          <template #append>
            <q-icon
              class="cursor-pointer"
              :name="showPassword[1] ? 'visibility_off' : 'visibility'"
              @click="togglePassword(1)"
            />
          </template>
        </q-input>
      </div>

      <!-- Save Button -->
      <div class="btn-save flex flex-center q-pb-md">
        <q-btn
          color="primary"
          style="width: 100px"
          :label="$t('btnSave')"
          :loading="loadingSave"
          :disable="loadingSave"
          @click="save"
        />
      </div>
    </div>
  </q-page>
</template>

<script>
import { defineComponent } from "vue";
import { mapGetters, mapActions } from "vuex";
import { api } from "src/boot/axios";
import MD5 from "crypto-js/md5";

export default defineComponent({
  name: "Change Password",

  data() {
    return {
      accountInfor: null,

      oldPassword: "",
      newPassword: "",
      confirmPassword: "",

      showPassword: [false, false],

      loadingSave: false,

      labelColorFocus: [],
    };
  },

  computed: {
    ...mapGetters("auth", ["getInformation"]),

    oldPasswordRules() {
      return [
        (val) => !!val || this.$t("oldPwdRequired"),
      ];
    },

    newPasswordRules() {
      return [
        (val) => !!val || this.$t("newPwdRequired"),
        (val) => val.length > 5 || this.$t("pwdLength"),
        (val) => val !== this.oldPassword || this.$t("pwdSame"),
      ];
    },

    confirmPasswordRules() {
      return [
        (val) => !!val || this.$t("confirmPwdRequired"),
        (val) => val.length > 5 || this.$t("confirmPwdLength"),
        (val) => val === this.newPassword || this.$t("confirmPwdSame"),
      ];
    },
  },

  methods: {
    ...mapActions("auth", ["validateToken", "logOut"]),

    togglePassword(index) {
      this.showPassword[index] = !this.showPassword[index];
    },

    validateForm() {
      return (
        this.$refs.oldRef?.validate() &&
        this.$refs.newRef?.validate() &&
        this.$refs.confirmRef?.validate()
      );
    },

    async save() {
      try {
        if (!this.validateForm()) {
          return;
        }

        this.loadingSave = true;

        const isAuth = await this.validateToken();

        if (!isAuth) {
          this.$router.replace("/login");
          return;
        }

        const result = await this.requestChangePassword();

        if (result?.success) {
          this.$q.notify({
            type: "positive",
            message: this.$t("Successfully updated"),
          });

          await this.logOut();

          setTimeout(() => {
            this.$router.replace("/login");
          }, 3000);

          return;
        }

        this.$q.notify({
          type: "negative",
          message: result?.message?.[0] || "Update failed",
        });
      } catch (error) {
        console.error("Change password error:", error);

        this.$q.notify({
          type: "negative",
          message: "Saving error!",
        });
      } finally {
        this.loadingSave = false;
      }
    },

    async requestChangePassword() {
      try {
        const payload = {
          oldPassword: MD5(this.oldPassword).toString(),
          newPassword: MD5(this.confirmPassword).toString(),
        };

        const response = await api.put(
          `/api/v1/account/change-password/${this.accountInfor.id}`,
          payload
        );

        return response.data;
      } catch (error) {
        if (error.response?.data) {
          return error.response.data;
        }

        return {
          success: false,
          message: ["Server Error!"],
        };
      }
    },

    mapInformation() {
      this.accountInfor = {
        ...this.getInformation,
      };
    },
  },

  created() {
    this.mapInformation();
  },
});
</script>

<style lang="scss" scoped>
.container {
  position: relative;
  width: 460px;
  height: 600px;

  background-color: $accent;
  border-radius: 10px;

  overflow: hidden;

  .btn-save {
    position: absolute;
    bottom: 0;
    width: 100%;
  }
}

@media (max-width: 600px) {
  .container {
    width: 95%;
    height: auto;
    min-height: 600px;
  }
}
</style>
