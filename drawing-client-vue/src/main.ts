/**
 * Entry point — registers <vuejs-grpc-client> as a Web Custom Element.
 *
 * When this script is loaded in any HTML page, the custom element becomes
 * available and can be used as:
 *
 *   <vuejs-grpc-client
 *       service-url="https://localhost:5001"
 *       scene="landschap"
 *       width="800"
 *       height="600">
 *   </vuejs-grpc-client>
 */
import { defineCustomElement } from 'vue';
import DrawingCanvas from './components/DrawingCanvas.ce.vue';

const DrawingCanvasElement = defineCustomElement(DrawingCanvas);

customElements.define('vuejs-grpc-client', DrawingCanvasElement);
