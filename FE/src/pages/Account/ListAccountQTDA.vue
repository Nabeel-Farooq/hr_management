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

  async validateAuth() {
    const isValid = await this.validateToken();

    if (!isValid) {
      this.$router.replace("/login");
      return false;
    }

    return true;
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

  reIndexArray(list) {
    list.forEach((row, index) => {
      row.index = index + 1;
    });
  },

  notifySuccess(message) {
    this.$q.notify({
      type: "positive",
      message,
    });
  },

  notifyError(message) {
    this.$q.notify({
      type: "negative",
      message,
    });
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
        api.post(`/api/v1/account/qtda`, payload)
      );

      if (!result?.success) {
        this.notifyError(result?.message?.[0] || "Insert failed");
        return;
      }

      if (this.listAccount.length >= this.pagination.rowsPerPage) {
        this.listAccount.pop();
      }

      this.listAccount.unshift(result.resource);

      this.reIndexArray(this.listAccount);

      this.closeModifyPopup();

      this.notifySuccess("Successfully added!");
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

  async openProject(accountId) {
    try {
      this.loadingProject = true;

      const isValid = await this.validateAuth();
      if (!isValid) return;

      this.tempAddAccountId = accountId;
      this.showProject = true;

      const result = await this.handleApiRequest(() =>
        api.get(`/api/v1/account/with-group/${accountId}`)
      );

      if (!result?.success) {
        this.notifyError(result?.message?.[0] || "Load project failed");
        return;
      }

      this.listProject = result?.resource?.groups || [];

      this.reIndexArray(this.listProject);
    } finally {
      this.loadingProject = false;
    }
  },

  async addProject() {
    const isValid = await this.validateAuth();
    if (!isValid) return;

    if (!this.tempAddProjectId) return;

    const payload = {
      accountId: this.tempAddAccountId,
      groupId: this.tempAddProjectId,
    };

    const result = await this.handleApiRequest(() =>
      api.post(`/api/v1/group/add-account`, payload)
    );

    if (!result?.success) {
      this.notifyError(result?.message?.[0] || "Add project failed");
      return;
    }

    if (this.listProject.length >= 10) {
      this.listProject.pop();
    }

    this.listProject.unshift(result.resource);

    this.reIndexArray(this.listProject);

    this.tempAddProjectId = null;

    this.notifySuccess("Successfully added!");
  },

  async removeProject(id) {
    const isValid = await this.validateAuth();
    if (!isValid) return;

    const payload = {
      accountId: this.tempAddAccountId,
      groupId: id,
    };

    const result = await this.handleApiRequest(() =>
      api.post(`/api/v1/group/remove-account`, payload)
    );

    if (!result?.success) {
      this.notifyError(result?.message?.[0] || "Remove failed");
      return;
    }

    await this.openProject(this.tempAddAccountId);

    this.notifySuccess("Successfully removed!");
  },

  closeProject() {
    this.tempAddProjectId = null;
    this.tempAddAccountId = null;

    this.listProject = [];
    this.listTempProject = [];

    this.showProject = false;
  },

  filterWork(val, update) {
    update(async () => {
      if (val?.length >= 2) {
        await this.findGroup(val);
      }
    });
  },

  async findGroup(keyword) {
    const isValid = await this.validateAuth();
    if (!isValid) return;

    const result = await this.handleApiRequest(() =>
      api.get(
        `/api/v1/group/search?filterName=${keyword?.trim() || ""}`
      )
    );

    if (!result?.success) {
      this.notifyError(result?.message?.[0] || "Search failed");
      return;
    }

    this.listTempProject = result.resource || [];
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
        this.notifyError(result?.message?.[0] || "Load failed");
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
        this.notifyError(result?.message?.[0] || "Load failed");
        return;
      }

      this.mappingPagination(result);
    } finally {
      this.loadingData = false;
    }
  },

  mappingPagination(resource) {
    this.listAccount = resource?.resource || [];

    this.reIndexArray(this.listAccount);

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
        api.delete(`/api/v1/account/qtda/${this.deleteObj?.id}`)
      );

      if (!result?.success) {
        this.notifyError(result?.message?.[0] || "Delete failed");
        return;
      }

      await this.getAccountWithFilter(false);

      this.notifySuccess("Successfully deleted!");
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
        api.put(`/api/v1/account/qtda/${this.editObj.id}`, payload)
      );

      if (!result?.success) {
        this.notifyError(result?.message?.[0] || "Update failed");
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

      this.closeModifyPopup();

      this.notifySuccess("Successfully updated!");
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
      viewer: "lime-10",
      "editor-kt": "cyan-10",
    };

    return colorMap[roleName] || "grey";
  },

  convertRoleStringToNumber(stringRole) {
    const roleMap = {
      admin: this.role.admin,
      "editor-qtns": this.role.editorQTNS,
      "editor-qtda": this.role.editorQTDA,
      viewer: this.role.viewer,
      "editor-kt": this.role.editorKT,
    };

    return roleMap[stringRole] || this.role.viewer;
  },
},
