        //chk lnk is mp3, mp4 or youtube
        function getLnkObj(lnk) {
          var lnkType;
            if (lnk.search(/\.mp4$/i) != -1) {
                lnkType = "mp4";
            }
            else if (lnk.search(/\.mp3$/i) != -1) {
                lnkType = "mp3";
            }
            else {
                lnkType = "youtube";
                ytlnk = crtYTEmbed(lnk);
                if (ytlnk != '') {
                    lnk = ytlnk;
                }
            }
            return {"lnk":lnk, "lnkType":lnkType};
        }
        
        //get browser info
        function who() {
            var ua = navigator.userAgent, tem,
                M = ua.match(/(opera|chrome|safari|firefox|msie|trident(?=\/))\/?\s*(\d+)/i) || [];
            if (/trident/i.test(M[1])) {
                tem = /\brv[ :]+(\d+)/g.exec(ua) || [];
                return 'IE ' + (tem[1] || '');
            }
            if (M[1] === 'Chrome') {
                tem = ua.match(/\b(OPR|Edge)\/(\d+)/);
                if (tem != null) return tem.slice(1).join(' ').replace('OPR', 'Opera');
            }
            M = M[2] ? [M[1], M[2]] : [navigator.appName, navigator.appVersion, '-?'];
            if ((tem = ua.match(/version\/(\d+)/i)) != null) M.splice(1, 1, tem[1]);
            //return M.join(' ');
            return ua;
        }

        function crtYTEmbed(url) {
            var regExp = /^.*(youtu.be\/|v\/|u\/\w\/|embed\/|watch\?v=|\&v=)([^#\&\?]*).*/;
            var match = url.match(regExp);

            if (match && match[2].length == 11) {
                return 'https://www.youtube.com/embed/' + match[2];
            } else {
                return '';
            }
        }
        function toggle(id) {
            var x = document.getElementById(id);
            if (x.style.display == "none") {
                x.style.display = "block";
            } else {
                x.style.display = "none";
            }
        }

        //__in showhide: true-> show
        function show(id, showhide) {
            var x = document.getElementById(id);
            if (showhide) {
                x.style.display = "block";
            } else {
                x.style.display = "none";
            }
        }

        //video1 controls
        function playPause(id) {
            ctrl = document.getElementById(id);
            if (ctrl.paused)
                ctrl.play();
            else
                ctrl.pause();
        }

        function makeBig(id) {
            ctrl = document.getElementById(id);
            w = Math.min(ctrl.width * 1.1, 1080);
            resize(id, w);
        }

        function makeSmall(id) {
            ctrl = document.getElementById(id);
            w = Math.max(ctrl.width * 0.9, 360);
            resize(id, w);
        }

        function makeNormal(id) {
            resize(id, 480);
        }
        function resize(id, w) {
            ctrl = document.getElementById(id);
            ctrl.width = w;
            if (id == "frame1") {
                ctrl.height = w * m_scale;
            }
        }