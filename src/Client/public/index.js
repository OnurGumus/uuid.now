      // Update footer year
      document.getElementById('year').textContent = new Date().getFullYear();

      // Characters used for uppercase v4 or time-based, including dash
      const charList = [
          "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
          "A", "B", "C", "D", "E", "F",
          "-"
      ];
      const FLIP_COUNT = 36; // 8-4-4-4-12

      const board = document.getElementById("flipBoard");

      // Build 36 flips
      for (let i = 0; i < FLIP_COUNT; i++) {
          const flip = document.createElement("div");
          flip.className = "flip";
          flip.dataset.value = "";
          flip.dataset.top = "0";

          const ul = document.createElement("ul");
          ul.style.top = "0px";
          charList.forEach(ch => {
              const li = document.createElement("li");
              li.textContent = ch;
              ul.appendChild(li);
          });
          flip.appendChild(ul);

          const overlay = document.createElement("div");
          overlay.className = "final-char";
          overlay.textContent = "-";
          flip.appendChild(overlay);

          board.appendChild(flip);
      }

      // Move UL down 35px
      function moveFlipDown(flipEl) {
          let currentTop = parseInt(flipEl.dataset.top, 10) || 0;
          currentTop -= 35;
          flipEl.dataset.top = currentTop;
          const ul = flipEl.querySelector("ul");
          ul.style.top = currentTop + "px";
      }
      // Reset UL to top=0
      function resetFlip(flipEl) {
          flipEl.dataset.top = "0";
          flipEl.querySelector("ul").style.top = "0px";
      }

      // Spin from currentVal to targetChar
      function switchChar(flipEl, targetChar) {
          const currentVal = flipEl.dataset.value || "";
          if (currentVal === targetChar) return;

          let idx = charList.indexOf(currentVal);
          if (idx < 0) idx = 0;
          const targetIdx = charList.indexOf(targetChar);
          if (targetIdx < 0) return; // not in charList => skip

          function step() {
              let nowVal = flipEl.dataset.value;
              if (nowVal === targetChar) return; // done
              let i = charList.indexOf(nowVal);
              i++;
              if (i >= charList.length) {
                  i = 0;
                  resetFlip(flipEl);
              } else {
                  moveFlipDown(flipEl);
              }
              const nextC = charList[i];
              flipEl.dataset.value = nextC;

              // Update overlay each step
              const overlay = flipEl.querySelector(".final-char");
              overlay.textContent = nextC;

              if (nextC !== targetChar) {
                  setTimeout(step, 80);
              }
          }
          step();
      }

      // Switch entire 36 flips to new GUID
      function switchToGuid(guidStr) {
          const flips = board.querySelectorAll(".flip");
          for (let i = 0; i < FLIP_COUNT; i++) {
              const flipEl = flips[i];
              const c = guidStr[i] || "-";
              setTimeout(() => {
                  switchChar(flipEl, c);
              }, i * 50);
          }
      }

      // Generate uppercase v4
      function getRandomV4Guid() {
          if (typeof crypto?.randomUUID === "function") {
              return crypto.randomUUID().toUpperCase();
          }
          // fallback
          let pat = ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
              (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
          );
          return pat.toUpperCase();
      }

      // Generate exactly 36 chars time-based => 8-4-4-4-12
      function getTimeBasedGuid() {
          // 8 from timestamp
          const secHex = Math.floor(Date.now() / 1000).toString(16).padStart(8, "0");
          // 12 random bytes => 24 hex
          const arr = new Uint8Array(12);
          crypto.getRandomValues(arr);
          let rndHex = "";
          for (let i = 0; i < 12; i++) {
              rndHex += arr[i].toString(16).padStart(2, "0");
          }
          // => 24 hex => format => 4-4-4-12 => total 24
          // plus the 8 from timestamp => total 32 hex, with 4 dashes => 36 length
          const part1 = rndHex.slice(0, 4);
          const part2 = rndHex.slice(4, 8);
          const part3 = rndHex.slice(8, 12);
          const part4 = rndHex.slice(12);

          // combined => 8-4-4-4-12
          return `${secHex}-${part1}-${part2}-${part3}-${part4}`.toUpperCase();
      }

      // The 4 main buttons
      const resetBtn = document.getElementById("resetBtn");
      const v4Btn = document.getElementById("v4Btn");
      const timeBtn = document.getElementById("timeBtn");
      const copyBtn = document.getElementById("copyBtn");

      resetBtn.addEventListener("click", () => {
          const zero = "00000000-0000-0000-0000-000000000000";
          switchToGuid(zero);
      });
      v4Btn.addEventListener("click", () => {
          const g = getRandomV4Guid();
          switchToGuid(g);
      });
      timeBtn.addEventListener("click", () => {
          const t = getTimeBasedGuid();
          switchToGuid(t);
      });

      // Copy => read .final-char from each flip, combine
      copyBtn.addEventListener("click", async () => {
          let finalStr = "";
          const flips = board.querySelectorAll(".flip");
          flips.forEach((f) => {
              const over = f.querySelector(".final-char");
              finalStr += (over?.textContent || "-");
          });
          try {
              await navigator.clipboard.writeText(finalStr);
              alert("Copied:\n" + finalStr);
          } catch (err) {
              console.error("Copy failed:", err);
              alert("Copy failed!");
          }
      });

      // Initialize with zero GUID
      document.addEventListener("DOMContentLoaded", () => {
          const zeroGUID = "00000000-0000-0000-0000-000000000000";
          switchToGuid(zeroGUID);
      });