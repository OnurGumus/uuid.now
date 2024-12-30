class CustomAnimatedText extends HTMLElement {
    static get observedAttributes() {
      return ['rotation'];
    }
  
    constructor() {
      super();
      const shadow = this.attachShadow({ mode: "open" });
  
      // Create container with clipping
      const container = document.createElement("div");
      container.classList.add("middle-text-container");
  
      // Create inner container for flex layout
      const innerContainer = document.createElement("div");
      innerContainer.classList.add("middle-text");
      container.appendChild(innerContainer); // Ensure innerContainer is appended
  
      // Add a slot for node input
      const slot = document.createElement("slot");
      container.appendChild(slot);
  
      // Handle slot content
      slot.addEventListener('slotchange', () => {
        const assignedNodes = slot.assignedNodes();
        innerContainer.innerHTML = ""; // Clear existing spans
        
        let totalLetters = 0;
        // First pass: count total letters for timing calculations
        assignedNodes.forEach(node => {
          if (node.nodeType === Node.ELEMENT_NODE) {
            totalLetters += node.textContent.trim().length;
          } else if (node.nodeType === Node.TEXT_NODE && node.textContent.trim()) {
            totalLetters += node.textContent.trim().length;
          }
        });
  
        const totalDuration = 1.3;
        const minDuration = 1.3;
        const step = (totalDuration - minDuration) / totalLetters;
        const initialOffset = 10;
        let currentIndex = 0;
  
        requestAnimationFrame(() => {
        assignedNodes.forEach(node => {
          if (node.nodeType === Node.ELEMENT_NODE || (node.nodeType === Node.TEXT_NODE && node.textContent.trim())) {
            const text = node.textContent.trim();
            const styles = node.nodeType === Node.ELEMENT_NODE ? 
              window.getComputedStyle(node) : 
              window.getComputedStyle(node.parentElement);
            
            const applyStyles = (element) => {
              ['color', 'fontFamily', 'fontSize', 'fontWeight', 'fontStyle', 'letterSpacing','lineHeight', 'textShadow'].forEach(prop => element.style[prop] = styles[prop]);
            };
  
            text.split('').forEach((char, i) => {
              const span = document.createElement("span");
              span.textContent = char === " " ? "\u00A0" : char;
              applyStyles(span);
  
              const delay = currentIndex * step;
              const duration = totalDuration - delay;
              
              span.classList.add(this.hasAttribute('rotation') ? 'with-rotation' : 'no-rotation');
              span.style.setProperty("--animation-delay", `${delay}s`);
              span.style.setProperty("--animation-duration", `${duration}s`);
              if (this.hasAttribute('rotation')) {
                span.style.setProperty("--initial-y", `${initialOffset * (currentIndex + 4)}px`);
              }
  
              innerContainer.appendChild(span);
              currentIndex++;
            });
  
            if (node !== assignedNodes[assignedNodes.length - 1]) {
              const space = document.createElement("span");
              space.textContent = "\u00A0";
              applyStyles(space);
              innerContainer.appendChild(space);
            }
          }
        });
        });
      });
  
      // Define styles within the Shadow DOM
      const style = document.createElement("style");
      style.textContent = /*css*/ `
          .middle-text-container {
            overflow: clip; /* Clipping */
            display: flex;
            justify-content: center;
            align-items: center;
            inline-size: 100%;
          }
  
          .middle-text {
            display: flex;
            justify-content: center;
            align-items: flex-end;
            font-size: inherit;
            font-weight: inherit;
            font-family: inherit;
            font-style: inherit;
            font-variant: inherit;
            font-stretch: inherit;
            line-height: inherit;
            letter-spacing: inherit;
            text-transform: inherit;
            text-shadow: inherit;
            color: inherit;
          }
  
          .middle-text span {
            display: inline-block;
            color: inherit; /* Changed from color: white */
            opacity: 0;
            transform: translateY(var(--initial-y));
          }
          
          .middle-text span.with-rotation {
            animation: raiseUpRotated var(--animation-duration) cubic-bezier(0.55,-0.26, 0, 0.55) forwards,
                       fadeIn calc(var(--animation-duration)*1) cubic-bezier(0.55,-0.26, 0, 0.55) forwards;
          }
  
          .middle-text span.no-rotation {
            animation: raiseUpSimple var(--animation-duration) cubic-bezier(0.55,-0.26, 0, 0.55) forwards,
                       fadeIn calc(var(--animation-duration)*1) cubic-bezier(0.55,-0.26, 0, 0.55) forwards;
          }
  
          @keyframes raiseUpRotated {
            100% {
              transform: translateY(0);
            }
          }
  
          @keyframes raiseUpSimple {
            0% { transform: translateY(100%); }
            100% { transform: translateY(0); }
          }
  
          @keyframes fadeIn {
            0% { opacity: 0; }
            100% { opacity: 1; }
          }
  
          /* Hide the original slot content */
          slot {
            display: none;
          }
        `;
  
      // Append styles and container to Shadow DOM
      shadow.appendChild(style);
      shadow.appendChild(container);
    }
  }
  
  // Define the custom element
  customElements.define("custom-animated-text", CustomAnimatedText);
  