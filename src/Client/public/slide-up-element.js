class SlideUpElement extends HTMLElement {
    connectedCallback() {
        const content = this.shadowRoot.querySelector('slot').assignedElements()[0];
        if (content) {
            content.style.transform = 'translateY(100%)';
            content.style.opacity = '0';

            requestAnimationFrame(() => {
                content.style.transform = 'translateY(0)';
                content.style.opacity = '1';
            });
        }
    }

    constructor() {
        super();
        const shadow = this.attachShadow({ mode: "open" });

        // Create container with clipping
        const container = document.createElement("div");
        container.classList.add("clip-container");

        // Add a slot for child content
        const slot = document.createElement("slot");
        container.appendChild(slot);

        const style = document.createElement("style");
        style.textContent = /*css*/ `
        .clip-container {
          overflow: clip;
          display: block;
          inline-size: 100%;
          block-size: 100%;
        }
  
        ::slotted(*) {
          display: block;
          transition: transform 1.3s cubic-bezier(0.55,-0.26, 0, 0.55),
                     opacity 1.3s cubic-bezier(0.55,-0.26, 0, 0.55);
        }
      `;

        shadow.appendChild(style);
        shadow.appendChild(container);
    }
}

customElements.define("slide-up-element", SlideUpElement);
