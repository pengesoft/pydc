(function (dd, orderSvr, autherSvr, systemUtil) {
  let timer;

  const app = new Vue({
    el: '#order',
    data() {
      return {
        isAllreadyOrdered: false,
        isLoading: false,
        isShowMsg: false,
        msg: '',
        now: new Date(),
        order: new Order(),
        orders: [],
        products: '',
        sysParams: new SysParams(),
        userId: '',
        userName: ''
      }
    },
    watch: {
      userId() {
        orderSvr.Get(this.userId, '', res => {
          this.isAllreadyOrdered = !!res;
          if (res) {
            this.order = res;
          } else {
            orderSvr.GetLast(this.userId, res2 => {
              if (res2) {
                this.order = res2;
              } else {
                this.order = new Order();
              }
            }, err => {
              this.showMsg('发生错误');
            });
          }
        }, err => {
          this.showMsg('发生错误');
        });
        orderSvr.GetSysParams(this.userId, (para) => {
            this.sysParams = para;
            this.products = getProductsText(para.Products);
          }
        );
      }
    },
    methods: {
      placeOrder() {
        this.isLoading = true;
        orderSvr.PlaceOrder(this.userId, this.order.Product, 1, this.order.Memo, (code) => {
            if (code === 0) {
              this.showMsg('下单成功');
              this.isAllreadyOrdered = true;
              this.isLoading = false;
            } else {
              this.showMsg('下单失败: ' + code);
              this.isLoading = false;
            }
          }
        );
      },
      cancelOrder() {
        this.isLoading = true;
        orderSvr.Delete(this.userId, () => {
            orderSvr.Get(this.userId, '', (order) => {
                if (!order) {
                  this.isAllreadyOrdered = false;
                  this.order.Product = '';
                  this.order.Memo = '';
                  this.showMsg('订单删除成功');
                  this.isLoading = false;
                }
              }, err => {
                this.isLoading = false;
              }
            );
          }, (err) => {
            this.isLoading = false;
            console.error($.toJSON(err));
          }
        );
      },
      finishOrder() {
        this.isLoading = true;
        orderSvr.FinishedOrder(this.userId, (ans) => {
            if (ans === 0) {
              this.showMsg('已通知其他小伙伴取餐');
            }
            this.isLoading = false;
          }, err => {
            this.isLoading = false;
          }
        );
      },
      modifySysParams() {
        const sysPars = systemUtil.clone(this.sysParams);
        sysPars.Products = getProductsArray(this.products);
        orderSvr.UpdateSysParams(this.userId, sysPars, 0, (code) => {
            if (code === 0) {
              this.sysParams = sysPars;
              this.showMsg('保存系统参数成功');
            } else
              this.showMsg('保存失败: ' + code);
          }
        );
      },
      refreshOrderList() {
        orderSvr.GetOrders(this.userId, '', 1, (orders) => {
            if (orders) {
              this.orders = orders._Items;
            }
          }
        );
      },
      showMsg(text) {
        if (timer) {
          clearTimeout(timer);
        }
        this.msg = text;
        this.isShowMsg = true;
        timer = setTimeout(() => {
          this.isShowMsg = false;
        }, 3000);
      }
    },
    computed: {
      isAdmin() {
        if (!this.userId || !this.sysParams) {
          return false;
        }
        return this.userId === this.sysParams.AdminUserId;
      },
      placeEnabled() {
        if (this.sysParams) {
          const dl = this.now.getHours() * 60 + this.now.getMinutes();
          return (dl > this.sysParams.Deadline - 120) && (dl < this.sysParams.Deadline);
        }
      },
      finishedEnabled() {
        if (this.sysParams) {
          const dl = this.now.getHours() * 60 + this.now.getMinutes();
          return dl > this.sysParams.Deadline;
        }
      },
      placeTime() {
        const dealTime = new Date(new Date().setHours(Math.floor(this.sysParams.Deadline / 60), this.sysParams.Deadline % 60, 0, 0) - (120 * 60 * 1000));
        const time = new Date(new Date(this.now.getTime()).setMilliseconds(0));
        return getMTimeStr((dealTime - time) / 1000);
      },
      endPlaceTime() {
        const dealTime = new Date(new Date().setHours(Math.floor(this.sysParams.Deadline / 60), this.sysParams.Deadline % 60, 0, 0));
        const time = new Date(new Date(this.now.getTime()).setMilliseconds(0));
        return getMTimeStr((dealTime - time) / 1000);
      }
    },
    created() {
      setInterval(() => {
        this.now = new Date();
      }, 1000);
    }
  });

  function getProductsText(products) {
    let s = '';
    for (let i = 0; i < products.length; i++) {
      s = s + products[i] + '\n';
    }
    return s;
  }

  function getProductsArray(products) {
    const ary = [];
    const ss = products.split('\n');
    for (let i = 0; i < ss.length; i++) {
      const s = ss[i].trim();
      if (s) {
        ary.push(s);
      }
    }
    return ary;
  }

  function getMTimeStr(time) {

    return `${getMTimeHour(time)} : ${getMTimeMin(time)} : ${getMTimeSecond(time)}`;

    function getMTimeHour(time) {
      const h = Math.floor(time / 60 / 60);
      return h >= 10 ? h : '0' + h;
    }

    function getMTimeMin(time) {
      const m = Math.floor(time / 60 % 60);
      return m >= 10 ? m : '0' + m;
    }

    function getMTimeSecond(time) {
      const s = time % 60;
      return s >= 10 ? s : '0' + s;
    }
  }

  function doLoginSucess(code) {
    autherSvr.GetAccessUser(code, (user) => {
        app.userId = user.userid;
        app.userName = user.name;
      }, (err) => {
        this.showMsg(systemUtil.getErrorMsg(err, false));
        console.error(systemUtil.getErrorMsg(err, true));
      }
    );
  }

  // app.userId = 'zp';
  // app.userName = '张鹏';

  dd.ready(function () {
    try {
      dd.runtime.permission.requestAuthCode({
        corpId: 'dinga1f24b7b258cf6c235c2f4657eb6378f', // 企业id
        onSuccess: function (info) {
          const code = info.code; // 通过该免登授权码可以获取用户身份
          doLoginSucess(code);
        },
        onFail: function (err) {
          alert('dd error: ' + JSON.stringify(err));
        }
      });
    } catch (err) {
      alert('加载异常');
    }
  });
}(dd, orderSvr, autherSvr, SysUtils));
