/*************************************************Jiahax-Start************************************/

    //Lambda-Select
    Array.prototype.select = Array.prototype.map || function (selector, context) {
        context = context || window;
        var array = [];
        for (var i = 0; i < l; i++) { array.push(selector.call(context, this[i], i, this)); }
        return array;
    };

    //Date Format
    Date.prototype.Format = function (format) {
        var o = {
            "M+": this.getMonth() + 1,
            "d+": this.getDate(),
            "H+": this.getHours(),
            "m+": this.getMinutes(),
            "s+": this.getSeconds(),
            "q+": Math.floor((this.getMonth() + 3) / 3),
            "S": this.getMilliseconds()
        };
        if (/(y+)/.test(format)) { format = format.replace(RegExp.$1, this.getFullYear().toString().substr(4 - RegExp.$1.length)); }
        for (var k in o) { if (new RegExp("(" + k + ")").test(format)) { format = format.replace(RegExp.$1, RegExp.$1.length === 1 ? o[k] : ("00" + o[k]).substr(o[k].toString().length)); } }
        return format;
    };

    //String.format
    String.prototype.format = function (arg) {
        return this.replace(new RegExp("{-?[0-9]+}", "g"), function (item) {
            var intVal = parseInt(item.substring(1, item.length - 1));
            var replace;
            if (intVal >= 0) { replace = arg[intVal]; }
            else if (intVal === -1) { replace = "{"; }
            else if (intVal === -2) { replace = "}"; }
            else { replace = ""; }
            return replace;
        });
    };

    //String.htmlDecode
    String.prototype.htmlDecode = function () {
        var temp = document.createElement("div");
        temp.innerHTML = this;
        var output = temp.innerText || temp.textContent;
        temp = null;
        return output;
    };

    //String.htmlEncode
    String.prototype.htmlEncode = function () {
        var temp = document.createElement("div");
        temp.textContent !== null ? temp.textContent = this : temp.innerText = this;
        var output = temp.innerHTML;
        temp = null;
        return output;
    };

    //格式化时间
    function _formatDateTime_Y() { WdatePicker({ lang: commConver.getWdatePickerLang(), dateFmt: "yyyy" }); }
    function _formatDateTime_M() { WdatePicker({ lang: commConver.getWdatePickerLang(), dateFmt: "yyyy-MM" }); }
    function _formatDateTime_d() { WdatePicker({ lang: commConver.getWdatePickerLang(), dateFmt: "yyyy-MM-dd" }); }
    function _formatDateTime_H() { WdatePicker({ lang: commConver.getWdatePickerLang(), dateFmt: "yyyy-MM" }); }
    function _formatDateTime_m() { WdatePicker({ lang: commConver.getWdatePickerLang(), dateFmt: "yyyy-MM-dd HH:mm" }); }
    function _formatDateTime_s() { WdatePicker({ lang: commConver.getWdatePickerLang(), dateFmt: "yyyy-MM-dd HH:mm:ss" }); }

    //格式化千分位化小数
    function _formatThousand(that, prompt) {
        var value = $.trim($(that).val()).replace(/,/g, "");
        var flag = /^([1-9][0-9]*|[0]\.\d{1,2}|[1-9][0-9]*\.\d{1,2})$/;
        if (!flag.test(value)) {
            $(that).val("");
            $i.alert({ message: prompt, setTime: 3000 });
            return;
        }
        $(that).val(parseFloat(value).toLocaleString());
    }

    //计算日期差DateDiff
    function _dateDiff(date1, date2, msg) {
        var responseMessage = { status: "1", diff: "", msg: "" };
        var milliseconds1 = Date.parse(new Date(date1));
        var milliseconds2 = Date.parse(new Date(date2));
        if (isNaN(milliseconds1) || isNaN(milliseconds2)) { responseMessage.status = "0"; responseMessage.msg = "Invalid Date"; }
        else if (milliseconds1 > milliseconds2) { responseMessage.status = "0"; responseMessage.msg = msg; }
        else { responseMessage.diff = Math.abs(parseInt((milliseconds2 - milliseconds1) / 1000 / 60 / 60 / 24)); }
        return responseMessage;
    }

    //反格式化千分位化小数
    function _deFormatThousand(that) { $(that).val($.trim($(that).val()).replace(/,/g, "")); }

    //动态Select2
    function _dynamicSelect(that, dynamicSelectModel) {
        $(that).select2({
            language: dynamicSelectModel.language || "zh-CN",
            placeholder: dynamicSelectModel.placeholder,
            placeholderOption: "first",
            multiple: false,
            allowClear: false,
            ajax: {
                url: dynamicSelectModel.url,
                dataType: "json",
                delay: 250,
                data: function (params) {
                    return {
                        query: params.term,
                        page: params.page
                    };
                },
                processResults: function (data, params) {
                    params.page = params.page || 1;

                    return {
                        results: data.items,
                        pagination: { more: (params.page * dynamicSelectModel.pageSize) < data.total }
                    };
                },
                cache: true
            },
            escapeMarkup: function (markup) { return markup; },
            minimumInputLength: 0,
        });
    }

    //初始化AutoTextArea
    function _initAutoTextArea() { $('div[type="textarea"]').find("textarea").each(function (index, element) { _autoTextarea(element); }); }

    //初始化动态表格.my-radio
    function _initDynamicRadio() {
        $('div[type="dyntable"]').find('td[type="radio"] input[type="radio"]').each(function (index, element) {
            $(element).attr("name", "radio-" + element.parentNode.parentNode.rowIndex);
        });
    }

    //初始化动态表格.my-select
    function _initDynamicSelect(that) { $(that).parents("tbody").find("tr:last-child .my-select").initMySelect(); }

    //百度编辑器Placeholder功能
    function getChromeVersion() {
        var arr = navigator.userAgent.split(' ');
        var chromeVersion = '';
        for (var i = 0; i < arr.length; i++) { if (/chrome/i.test(arr[i])) { chromeVersion = arr[i]; } }
        if (chromeVersion) { return Number(chromeVersion.split('/')[1].split('.')[0]); } else { return false; }
    }
    function _placeholder(element, placeholderText) {
        var chromeVersion = getChromeVersion();
        if (chromeVersion === false || chromeVersion < 73) { console.log("Chrome Version: " + chromeVersion); return; }
        var placeholderHtml = '<input class="placeholder" type="text" placeholder="' + placeholderText + '" style="width:100%;" />';
        $(element).parent().prepend(placeholderHtml);
        var controlId = $(element).attr("id");
        var ue = UE.getEditor(controlId);
        var placeholderDom = $(element).parent().find(".placeholder");
        $(placeholderDom).next().css({ "display": "none", "z-index": "0" });
        $(placeholderDom).focus(function () { $(placeholderDom).hide(); $(placeholderDom).next().show(); ue.focus(); });
        if ($(element).parent().find("textarea").hasClass("view-uied-content") || $(element).val() !== "") { $(placeholderDom).hide(); $(placeholderDom).next().show(); }
        ue.addListener("blur", function () { if (ue.hasContents()) { $(placeholderDom).hide(); $(placeholderDom).next().show(); } else { $(placeholderDom).show(); $(placeholderDom).next().hide(); } });
        if (controlId.startsWith("form-field-SINGLELINE")) {
            ue.addListener("keydown", function (type, event) { if (event.which === 13) { event.cancelBubble = true; event.preventDefault(); event.stopPropagation(); } });
            ue.ready(function () {
                ue.setHeight(35);
                ue.iframe.contentWindow.document.getElementsByTagName("html")[0].setAttribute("style", "height:100%;");
                ue.body.setAttribute("style", "display:flex;align-items:center;margin:0;height:100%;font-size:14px;");
                ue.body.firstChild.setAttribute("style", "margin:0;vertical-align:middle;line-height:2em;height:2em;");
                ue.blur();
            });
        }
        if (controlId.startsWith("form-field-MULTILINE")) {
            ue.ready(function () {
                ue.iframe.contentWindow.document.getElementsByTagName("html")[0].setAttribute("style", "height:100%;");
                ue.body.setAttribute("style", "margin:0;padding:0.5em 0;height:100%;font-size:14px;box-sizing:border-box;");
                ue.body.firstChild.setAttribute("style", "margin:0;vertical-align:middle;line-height:2em;height:2em;");
                ue.blur();
            });
        }
    }

    //请输入正整数
    function _pleaseEnterInteger(that, prompt) { var regExp = /^[1-9]\d*$/; if (!regExp.exec(that.value)) { $i.alert(prompt); that.value = ""; } }

    //设置控件必填
    function _appendRequired(that) {
        if ($(that).find("span.profile-span").length === 0) {
            $(that).addClass("cls-required");
            $(that).children().addClass("cls-required").addClass("required");
            if ($(that).find("span.cls-required-ico").length === 0) { $(that).append("<span class=\"cls-required-ico\">*</span>"); }
            switch ($(that).attr("type")) {
                case "select":
                    $(that).find("select").addClass("required"); break;
                case "fckedit":
                    $(that).find(".uied-content,.fck-uied-content").addClass("required"); break;
                default: $(that).find("input").addClass("required"); break;
            }
        }
        else { $(that).children().addClass("cls-required").find("span.profile-span").addClass("view-required"); }
    }
    //移除控件必填
    function _removeRequired(that) {
        $(that).removeClass("required").removeClass("cls-required");
        $(that).find("*").removeClass("required").removeClass("cls-required");
        $(that).find("span.profile-span").removeClass("view-required");
        $(that).find("span.cls-required-ico").remove();
    }

    /*************************************************Jiahax-End************************************/
