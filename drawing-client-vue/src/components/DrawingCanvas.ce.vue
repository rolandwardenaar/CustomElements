<template>
  <div class="drawing-canvas-container">
    <canvas ref="canvasRef" :width="canvasWidth" :height="canvasHeight"></canvas>
    <div class="drawing-controls">
      <select v-model="selectedScene" @change="onSceneChange">
        <option v-for="scene in availableScenes" :key="scene" :value="scene">
          {{ scene }}
        </option>
      </select>
      <button :disabled="isDrawing" @click="drawScene">
        {{ isDrawing ? 'Tekenen...' : '▶ Teken' }}
      </button>
    </div>
    <div v-if="statusMessage" class="drawing-status">{{ statusMessage }}</div>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, onMounted, watch } from 'vue';
import { DrawingCanvasClient } from '../grpc/client';
import { executeBatch, clearCanvas } from '../canvas/renderer';
import type { DecodedDrawCommand } from '../proto/schema';

export default defineComponent({
  name: 'DrawingCanvas',

  props: {
    serviceUrl: { type: String, default: '' },
    scene: { type: String, default: 'landschap' },
    width: { type: [Number, String], default: 800 },
    height: { type: [Number, String], default: 600 },
    autoPlay: { type: [Boolean, String], default: true },
  },

  setup(props) {
    const canvasRef = ref<HTMLCanvasElement | null>(null);
    const canvasWidth = ref(Number(props.width) || 800);
    const canvasHeight = ref(Number(props.height) || 600);
    const selectedScene = ref(props.scene || 'landschap');
    const availableScenes = ref<string[]>([]);
    const isDrawing = ref(false);
    const statusMessage = ref('');

    let client: DrawingCanvasClient | null = null;

    // Parse auto-play: accept boolean or string "true"/"false"
    function isAutoPlay(): boolean {
      if (typeof props.autoPlay === 'boolean') return props.autoPlay;
      return props.autoPlay !== 'false' && props.autoPlay !== '0';
    }

    async function loadScenes(): Promise<void> {
      if (!client) return;
      try {
        availableScenes.value = await client.getAvailableScenes();
      } catch (err) {
        statusMessage.value = `Fout bij laden scènes: ${(err as Error).message}`;
      }
    }

    async function drawScene(): Promise<void> {
      const canvas = canvasRef.value;
      if (!client || !canvas || isDrawing.value) return;

      const ctx = canvas.getContext('2d');
      if (!ctx) return;

      isDrawing.value = true;
      statusMessage.value = `Tekenen: ${selectedScene.value}...`;

      try {
        clearCanvas(canvas);

        const batch: DecodedDrawCommand[] = [];
        const batchSize = 20;

        for await (const cmd of client.streamScene(selectedScene.value)) {
          batch.push(cmd);

          if (batch.length >= batchSize) {
            executeBatch(ctx, batch);
            batch.length = 0;
          }
        }

        // Flush remaining
        if (batch.length > 0) {
          executeBatch(ctx, batch);
        }

        statusMessage.value = `✓ ${selectedScene.value} voltooid`;
      } catch (err) {
        statusMessage.value = `Fout: ${(err as Error).message}`;
      } finally {
        isDrawing.value = false;
      }
    }

    function onSceneChange(): void {
      if (isAutoPlay()) {
        drawScene();
      }
    }

    // Reflect attribute changes to internal state
    watch(() => props.width, (v) => { canvasWidth.value = Number(v) || 800; });
    watch(() => props.height, (v) => { canvasHeight.value = Number(v) || 600; });
    watch(() => props.scene, (v) => {
      if (v) {
        selectedScene.value = v;
        if (isAutoPlay()) drawScene();
      }
    });

    onMounted(async () => {
      if (!props.serviceUrl) {
        statusMessage.value = '⚠ Geen service-url opgegeven.';
        return;
      }

      client = new DrawingCanvasClient(props.serviceUrl);
      await loadScenes();

      if (isAutoPlay()) {
        await drawScene();
      }
    });

    return {
      canvasRef,
      canvasWidth,
      canvasHeight,
      selectedScene,
      availableScenes,
      isDrawing,
      statusMessage,
      drawScene,
      onSceneChange,
    };
  },
});
</script>

<style>
.drawing-canvas-container {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  font-family: 'Segoe UI', system-ui, sans-serif;
}

canvas {
  border: 2px solid #333;
  border-radius: 8px;
  background: #f0f8ff;
  max-width: 100%;
  height: auto;
}

.drawing-controls {
  display: flex;
  gap: 0.5rem;
  align-items: center;
}

.drawing-controls select {
  padding: 0.4rem 0.8rem;
  border: 1px solid #ccc;
  border-radius: 6px;
  font-size: 0.9rem;
  background: #fff;
  text-transform: capitalize;
}

.drawing-controls button {
  padding: 0.4rem 1rem;
  border: none;
  border-radius: 6px;
  background: #42b883;
  color: #fff;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.2s;
}

.drawing-controls button:hover:not(:disabled) {
  background: #359968;
}

.drawing-controls button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.drawing-status {
  font-size: 0.8rem;
  color: #666;
  padding: 0.2rem 0;
}
</style>
