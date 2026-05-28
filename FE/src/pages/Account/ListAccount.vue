methods: {
  ...mapActions("auth", ["useRefreshToken", "validateToken"]),

  async handleApiRequest(callback) {
    try {
      const response = await callback();
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

  validateForm() {
    return (
      this.$refs.oneRef?.validate() &&
      this.$refs.twoRef?.validate() &&
      this.$refs.threeRef?.validate() &&
      this.$refs.fourRef?.validate() &&
      this.$refs.fiveRef?.validate()
    );
  },

  resetTempAccount() {
    this.tempAccountResource = {
      userName: "",
      password: this.tempPwd,
      name: "",
      email: "",
      role: 4,
    };
  },

  async validateAuth() {
    const isValid = await this.validateToken();

    if (!isValid) {
      this.$router.replace("/login");
      return false;
    }

    return true;
  },

  openInsert() {
    this.resetTempAccount();

    this.showInsert = true;
    this.show = true;
  },

  async saveInsert() {
    try {
      if (!this.validateForm()) return;

      this.accountProcess = true;

      const isValid = await this.validateAuth();
      if (!isValid) return;

      const payload = {
        name: this.tempAccountResource.name,
        userName: this.tempAccountResource.userName,
        password: MD5(this.tempAccountResource.password).toString(),
        role: this.tempAccountResource.role,
        email: this.tempAccountResource.email,
      };

      const result = await this.handleApiRequest(() =>
        api.post(`/api/v1/account`, payload)
      );

      if (!result?.success) {
        this.$q.notify({
          type: "negative",
          message: result?.message?.[0] || "Insert failed",
        });

        return;
      }

      if (this.listAccount.length >= this.pagination.rowsPerPage) {
        this.listAccount.pop();
      }

      this.listAccount.unshift(result.resource);

      this.listAccount.forEach((row, index) => {
        row.index = index + 1;
      });

      this.closeModifyPopup();

      this.$q.notify({
        type: "positive",
        message: "Successfully added!",
      });
    } finally {
      this.accountProcess = false;
    }
  },

  closeModifyPopup() {
    this.showDelete = false;
    this.showEdit = false;
    this.showInsert = false;
    this.show = false;

    this.editObj = null;
    this.deleteObj = null;
  },

  async getAccount() {
    try {
      this.loadingData = true;

      const isValid = await this.validateAuth();
      if (!isValid) return;

      const result = await this.handleApiRequest(() =>
        api.get(`/api/v1/account?page=1&pageSize=10`)
      );

      if (!result?.success) {
        this.$q.notify({
          type: "negative",
          message: result?.message?.[0] || "Load failed",
        });

        return;
      }

      this.mappingPagination(result);
    } finally {
      this.loadingData = false;
    }
  },

  async getAccountWithFilter(props) {
    try {
      this.loadingData = true;

      const isValid = await this.validateAuth();
      if (!isValid) return;

      const { page, rowsPerPage } = props?.pagination || {
        page: 1,
        rowsPerPage: this.pagination.rowsPerPage,
      };

      const result = await this.handleApiRequest(() =>
        api.post(
          `/api/v1/account/pagination?page=${page}&pageSize=${rowsPerPage}`,
          this.filter
        )
      );

      if (!result?.success) {
        this.$q.notify({
          type: "negative",
          message: result?.message?.[0] || "Load failed",
        });

        return;
      }

      this.mappingPagination(result);
    } finally {
      this.loadingData = false;
    }
  },

  mappingPagination(resource) {
    this.listAccount = resource?.resource || [];

    this.listAccount.forEach((row, index) => {
      row.index = index + 1;
    });

    Object.assign(this.pagination, {
      page: resource.page,
      rowsPerPage: resource.pageSize,
      rowsNumber: resource.totalRecords,
      firstPage: resource.firstPage,
      lastPage: resource.lastPage,
      nextPage: resource.nextPage,
      previousPage: resource.previousPage,
      totalPages: resource.totalPages,
    });
  },

  showName(text = "") {
    return text.length > 20
      ? `${text.slice(0, 20)}...`
      : text;
  },

  openDelete(id) {
    this.deleteObj = this.listAccount.find((x) => x.id == id);
    this.showDelete = true;
  },

  async deleteAccount() {
    try {
      this.accountProcess = true;

      const isValid = await this.validateAuth();
      if (!isValid) return;

      const result = await this.handleApiRequest(() =>
        api.delete(`/api/v1/account/${this.deleteObj?.id}`)
      );

      if (!result?.success) {
        this.$q.notify({
          type: "negative",
          message: result?.message?.[0] || "Delete failed",
        });

        return;
      }

      await this.getAccountWithFilter(false);

      this.$q.notify({
        type: "positive",
        message: "Successfully deleted!",
      });
    } finally {
      this.accountProcess = false;
      this.showDelete = false;
    }
  },

  openEdit(id) {
    this.editObj = this.listAccount.find((x) => x.id == id);

    if (!this.editObj) return;

    this.tempAccountResource.name = this.editObj.name;
    this.tempAccountResource.userName = this.editObj.userName;
    this.tempAccountResource.password = this.tempPwd;
    this.tempAccountResource.role = this.convertRoleStringToNumber(
      this.editObj.role
    );
    this.tempAccountResource.email = this.editObj.email;

    this.showEdit = true;
    this.show = true;
  },

  async saveUpdate() {
    try {
      if (!this.validateForm()) return;

      this.accountProcess = true;

      const isValid = await this.validateAuth();
      if (!isValid) return;

      const payload = {
        name: this.tempAccountResource.name,
        password: MD5(this.tempAccountResource.password).toString(),
        userName: this.tempAccountResource.userName,
        role: this.tempAccountResource.role,
        email: this.tempAccountResource.email,
      };

      const result = await this.handleApiRequest(() =>
        api.put(`/api/v1/account/${this.editObj.id}`, payload)
      );

      if (!result?.success) {
        this.$q.notify({
          type: "negative",
          message: result?.message?.[0] || "Update failed",
        });

        return;
      }

      const index = this.listAccount.findIndex(
        (x) => x.id == this.editObj.id
      );

      if (index !== -1) {
        const numberIndex = this.listAccount[index].index;

        this.listAccount[index] = {
          ...result.resource,
          index: numberIndex,
        };
      }

      this.editObj = null;

      this.closeModifyPopup();

      this.$q.notify({
        type: "positive",
        message: "Successfully updated!",
      });
    } finally {
      this.accountProcess = false;
    }
  },

  convertDateTimeFormat(dateTime, stringFormat = "YYYY-MM-DD hh:mm") {
    if (!dateTime) return "";

    return date.formatDate(dateTime, stringFormat);
  },

  getColorRole(roleName) {
    const colorMap = {
      admin: "green-10",
      "editor-qtns": "red-10",
      "editor-qtda": "purple-10",
      "editor-kt": "cyan-10",
      viewer: "lime-10",
    };

    return colorMap[roleName] || "grey";
  },

  convertRoleStringToNumber(stringRole) {
    const roleMap = {
      admin: this.role.admin,
      "editor-qtns": this.role.editorQTNS,
      "editor-qtda": this.role.editorQTDA,
      "editor-kt": this.role.editorKT,
      viewer: this.role.viewer,
    };

    return roleMap[stringRole] || this.role.viewer;
  },

  preventDeleteSelfAccount(id) {
    const idAccount = this.getInformation?.id;

    return id !== idAccount && id !== -1;
  },
},
