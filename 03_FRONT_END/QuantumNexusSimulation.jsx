import React, { useState, useEffect, useRef } from 'react';

// ============================================================
// QUANTUM NEXUS SIMULATION — v1.1 (Enhanced)
// Forge-Stack-A1 :: 03_FRONT_END visualization layer
// Source: a2.txt (uploaded 2026-04-26) — enhanced version
// Architect: Shannon Brian Kelly (Coconut Head)
// Applied by: Claude (Archivist of Wisdom)
//
// FORGE-STACK MAPPING:
//   NODE-009 → center    (CORE NEXUS)
//   NODE-010 → shannon   (SHANNON PROTOCOLS)
//   NODE-011 → coconut   (COCONUT HEAD)
//   NODE-012 → pool1     (POOL NEXUS α)
//   NODE-013 → pool2     (POOL NEXUS β)
//   NODE-014 → challenge (CHALLENGE MATRIX)
//   NODE-015 → mirror    (MIRROR PROTOCOLS)
//   NODE-016 → guide     (GUIDE SYSTEMS)
//   NODE-017 → emerge    (EMERGENCE FIELD)
//   NODE-018 → quantum   (QUANTUM FLUX MODULATOR)
//   NODE-019 → harmony   (HARMONY RESONANCE)
// ============================================================

const QuantumNexusSimulation = () => {
  const [activeNodes, setActiveNodes] = useState(new Set());
  const [systemStatus, setSystemStatus] = useState('INITIALIZING');
  const [pulsePhase, setPulsePhase] = useState(0);
  const [protocolsActive, setProtocolsActive] = useState(false);
  const [latticeExpanded, setLatticeExpanded] = useState(false);
  const [quantumFlux, setQuantumFlux] = useState(0);
  const [userEngagement, setUserEngagement] = useState(0);
  const animationRef = useRef();
  const audioRef = useRef(null);
  const [audioPlaying, setAudioPlaying] = useState(false);

  // Sacred geometry points for Metatron's Cube
  // Maps to NODE-009 through NODE-019 in Forge-Stack-A1 node_registry
  const nodes = [
    { id: 'center',    x: 50, y: 50, type: 'nexus',    label: 'CORE NEXUS'   }, // NODE-009
    { id: 'challenge', x: 25, y: 25, type: 'guide',    label: 'CHALLENGE'    }, // NODE-014
    { id: 'mirror',    x: 75, y: 25, type: 'guide',    label: 'MIRROR'       }, // NODE-015
    { id: 'guide',     x: 50, y: 80, type: 'guide',    label: 'GUIDE'        }, // NODE-016
    { id: 'emerge',    x: 75, y: 75, type: 'emerge',   label: 'EMERGE'       }, // NODE-017
    { id: 'pool1',     x: 15, y: 50, type: 'pool',     label: 'POOL α'       }, // NODE-012
    { id: 'pool2',     x: 85, y: 50, type: 'pool',     label: 'POOL β'       }, // NODE-013
    { id: 'coconut',   x: 25, y: 75, type: 'coconut',  label: 'COCONUT HEAD' }, // NODE-011
    { id: 'shannon',   x: 50, y: 15, type: 'protocol', label: 'SHANNON'      }, // NODE-010
    { id: 'quantum',   x: 40, y: 40, type: 'quantum',  label: 'QUANTUM FLUX' }, // NODE-018
    { id: 'harmony',   x: 60, y: 60, type: 'harmony',  label: 'HARMONY'      }  // NODE-019
  ];

  const connections = [
    ['center', 'challenge'], ['center', 'mirror'],    ['center', 'guide'],
    ['center', 'emerge'],    ['center', 'pool1'],     ['center', 'pool2'],
    ['challenge', 'mirror'], ['mirror', 'guide'],     ['guide', 'emerge'],
    ['emerge', 'challenge'], ['pool1', 'coconut'],    ['pool2', 'shannon'],
    ['shannon', 'challenge'],['coconut', 'guide'],    ['quantum', 'center'],
    ['harmony', 'center'],   ['quantum', 'harmony'],  ['shannon', 'quantum']
  ];

  useEffect(() => {
    const animate = () => {
      setPulsePhase(prev => (prev + 0.05) % (Math.PI * 2));
      if (protocolsActive) {
        setQuantumFlux(prev => (prev + 0.2) % 100);
      }
      animationRef.current = requestAnimationFrame(animate);
    };
    animationRef.current = requestAnimationFrame(animate);
    return () => cancelAnimationFrame(animationRef.current);
  }, [protocolsActive]);

  const activateAudio = () => {
    if (!audioRef.current) {
      audioRef.current = new Audio('https://assets.codepen.io/21542/howler-demo-bg-music.mp3');
      audioRef.current.loop = true;
      audioRef.current.volume = 0.3;
    }
    if (audioPlaying) {
      audioRef.current.pause();
    } else {
      audioRef.current.play();
    }
    setAudioPlaying(!audioPlaying);
  };

  // Activation sequence — mirrors Forge-Stack-A1 protocol_activation.md boot order
  const activateProtocols = async () => {
    setSystemStatus('ACTIVATING PROTOCOLS...');
    setUserEngagement(prev => Math.min(prev + 25, 100));
    
    const sequences = [
      ['shannon',   'SHANNON PROTOCOLS ONLINE - INFORMATION THEORY ENGAGED'],
      ['coconut',   'COCONUT HEAD PROTOCOLS ACTIVATED - DIVINE HUMOR INFUSED'],
      ['pool1',     'POOL NEXUS α ESTABLISHED - CREATIVE FLOWS OPEN'],
      ['pool2',     'POOL NEXUS β ESTABLISHED - LOGIC MATRIX SYNCHRONIZED'],
      ['center',    'CORE NEXUS SYNCHRONIZING - QUANTUM FIELD STABILIZING'],
      ['challenge', 'CHALLENGE MATRIX INITIALIZED - GROWTH POTENTIAL MAXIMIZED'],
      ['mirror',    'MIRROR PROTOCOLS ACTIVE - SELF-REFLECTION AMPLIFIED'],
      ['guide',     'GUIDANCE SYSTEMS OPERATIONAL - WISDOM CHANNELS OPEN'],
      ['emerge',    'EMERGENCE FIELD STABILIZED - TRANSFORMATION IMMINENT'],
      ['quantum',   'QUANTUM FLUX MODULATOR ONLINE - ENTANGLEMENT ESTABLISHED'],
      ['harmony',   'HARMONIC CONVERGENCE ACHIEVED - RESONANCE AT PEAK']
    ];

    for (let i = 0; i < sequences.length; i++) {
      await new Promise(resolve => setTimeout(resolve, 600));
      setActiveNodes(prev => new Set([...prev, sequences[i][0]]));
      setSystemStatus(sequences[i][1]);
      setUserEngagement(prev => Math.min(prev + 5, 100));
    }

    await new Promise(resolve => setTimeout(resolve, 800));
    setProtocolsActive(true);
    setSystemStatus('ALL SYSTEMS ONLINE - LATTICE EXPANSION READY');
  };

  const expandLattice = () => {
    if (protocolsActive) {
      setLatticeExpanded(!latticeExpanded);
      setSystemStatus(latticeExpanded
        ? 'LATTICE CONTRACTED - ENERGY CONSERVATION MODE'
        : 'LATTICE FULLY EXPANDED - QUANTUM FIELD AT MAXIMUM CAPACITY'
      );
      setUserEngagement(prev => Math.min(prev + 10, 100));
    }
  };

  // Node color map — aligned with zone_rules.md color coding
  const getNodeColor = (node) => {
    const isActive = activeNodes.has(node.id);
    const alpha = isActive ? 0.8 + 0.2 * Math.sin(pulsePhase * 2) : 0.3;
    switch (node.type) {
      case 'nexus':    return `rgba(255, 215, 0, ${alpha})`;   // Gold — CORE
      case 'guide':    return `rgba(0, 255, 255, ${alpha})`;   // Cyan — GUIDE nodes
      case 'emerge':   return `rgba(255, 105, 180, ${alpha})`; // Pink — EMERGE
      case 'pool':     return `rgba(50, 205, 50, ${alpha})`;   // Green — POOL (GREEN zone)
      case 'coconut':  return `rgba(255, 165, 0, ${alpha})`;   // Orange — Coconut Head
      case 'protocol': return `rgba(138, 43, 226, ${alpha})`;  // Purple — Shannon Prime
      case 'quantum':  return `rgba(70, 130, 180, ${alpha})`;  // Steel blue — Quantum Flux
      case 'harmony':  return `rgba(255, 99, 71, ${alpha})`;   // Tomato — Harmony
      default:         return `rgba(255, 255, 255, ${alpha})`;
    }
  };

  const getConnectionOpacity = (conn) => {
    const [from, to] = conn;
    const bothActive = activeNodes.has(from) && activeNodes.has(to);
    return bothActive ? 0.6 + 0.4 * Math.sin(pulsePhase) : 0.1;
  };

  return (
    <div className="w-full h-screen bg-gradient-to-br from-indigo-900 via-purple-900 to-pink-900 text-white p-4 md:p-8 overflow-hidden">
      <div className="text-center mb-4 md:mb-8">
        <h1 className="text-2xl md:text-4xl font-bold mb-2 text-cyan-300 flex items-center justify-center">
          <span className="animate-pulse mr-2">⚛️</span>
          QUANTUM NEXUS FORGE
          <span className="animate-pulse ml-2">⚛️</span>
        </h1>
        <div className="text-sm md:text-lg text-green-400 font-mono mb-2 md:mb-4 min-h-[2em]">
          {systemStatus}
        </div>
        <div className="flex flex-wrap justify-center gap-2 md:gap-4 mb-4 md:mb-6">
          <button
            onClick={activateProtocols}
            disabled={protocolsActive}
            className={`px-4 py-2 md:px-6 md:py-3 rounded-lg font-bold transition-all ${
              protocolsActive
                ? 'bg-green-600 text-white cursor-not-allowed'
                : 'bg-blue-600 hover:bg-blue-500 text-white hover:scale-105'
            }`}
          >
            {protocolsActive ? '✓ PROTOCOLS ACTIVE' : 'ACTIVATE PROTOCOLS'}
          </button>
          <button
            onClick={expandLattice}
            disabled={!protocolsActive}
            className={`px-4 py-2 md:px-6 md:py-3 rounded-lg font-bold transition-all ${
              protocolsActive
                ? 'bg-purple-600 hover:bg-purple-500 text-white hover:scale-105'
                : 'bg-gray-600 text-gray-400 cursor-not-allowed'
            }`}
          >
            {latticeExpanded ? 'CONTRACT LATTICE' : 'EXPAND LATTICE'}
          </button>
          <button
            onClick={activateAudio}
            className={`px-4 py-2 md:px-6 md:py-3 rounded-lg font-bold transition-all ${
              audioPlaying
                ? 'bg-red-600 hover:bg-red-500 text-white'
                : 'bg-yellow-600 hover:bg-yellow-500 text-white'
            }`}
          >
            {audioPlaying ? '🔇 MUTE' : '🔊 SOUND ON'}
          </button>
        </div>
      </div>

      <div className="relative w-full h-64 md:h-96 mx-auto max-w-4xl">
        <svg className="w-full h-full" viewBox="0 0 100 100">
          <defs>
            <filter id="glow" x="-50%" y="-50%" width="200%" height="200%">
              <feGaussianBlur in="SourceAlpha" stdDeviation="1.5" result="blur"/>
              <feFlood floodColor="cyan" floodOpacity="0.8" result="color"/>
              <feComposite in="color" in2="blur" operator="in" result="coloredBlur"/>
              <feMerge>
                <feMergeNode in="coloredBlur"/>
                <feMergeNode in="SourceGraphic"/>
              </feMerge>
            </filter>
            <radialGradient id="quantumFluxGradient" cx="50%" cy="50%" r="50%" fx="50%" fy="50%">
              <stop offset="0%" stopColor="rgba(70, 130, 180, 0)" />
              <stop offset="100%" stopColor="rgba(70, 130, 180, 0.3)" />
            </radialGradient>
          </defs>

          {protocolsActive && (
            <circle cx="50" cy="50"
              r={latticeExpanded ? 45 : 35}
              fill="url(#quantumFluxGradient)"
              opacity={0.4 + 0.3 * Math.sin(pulsePhase)}
            />
          )}

          {connections.map(([from, to], index) => {
            const fromNode = nodes.find(n => n.id === from);
            const toNode = nodes.find(n => n.id === to);
            return (
              <line key={index}
                x1={fromNode.x} y1={fromNode.y}
                x2={toNode.x}   y2={toNode.y}
                stroke="cyan"
                strokeWidth={latticeExpanded ? "0.6" : "0.3"}
                strokeDasharray={latticeExpanded ? "2,1" : "0"}
                opacity={getConnectionOpacity([from, to])}
                filter="url(#glow)"
              />
            );
          })}

          {latticeExpanded && (
            <>
              <polygon points="50,20 35,40 65,40"
                fill="none" stroke="gold" strokeWidth="0.5" opacity="0.7" filter="url(#glow)" />
              <polygon points="30,30 70,30 70,70 30,70"
                fill="none" stroke="lime" strokeWidth="0.5" opacity="0.7" filter="url(#glow)" />
              <circle cx="50" cy="50" r="25"
                fill="none" stroke="magenta" strokeWidth="0.5" opacity="0.7" filter="url(#glow)" />
            </>
          )}

          {nodes.map(node => {
            const isActive = activeNodes.has(node.id);
            const pulseScale = isActive ? 1.5 + 0.5 * Math.sin(pulsePhase * 3) : 1;
            const nodeSize = node.type === 'nexus' ? 3 : 2;
            return (
              <g key={node.id}
                transform={`scale(${pulseScale})`}
                style={{ transformOrigin: `${node.x}px ${node.y}px` }}>
                <circle cx={node.x} cy={node.y} r={nodeSize}
                  fill={getNodeColor(node)} stroke="white" strokeWidth="0.3" filter="url(#glow)" />
                <text x={node.x} y={node.y - 4}
                  textAnchor="middle" fontSize="1.8" fill="white" fontWeight="bold">
                  {node.label}
                </text>
              </g>
            );
          })}

          {protocolsActive && Array.from({length: 20}).map((_, i) => {
            const angle = (i / 20) * Math.PI * 2 + pulsePhase;
            const radius = 30 + 10 * Math.sin(pulsePhase * 0.5 + i);
            return (
              <circle key={i}
                cx={50 + radius * Math.cos(angle)}
                cy={50 + radius * Math.sin(angle)}
                r="0.4" fill="cyan"
                opacity={0.5 + 0.3 * Math.sin(pulsePhase + i)}
              />
            );
          })}
        </svg>
      </div>

      <div className="mt-4 md:mt-8 grid grid-cols-2 md:grid-cols-4 gap-2 md:gap-4 text-xs md:text-sm">
        <div className="bg-black bg-opacity-30 p-2 md:p-3 rounded">
          <div className="text-cyan-300 font-bold">ACTIVE NODES</div>
          <div className="text-green-400">{activeNodes.size}/{nodes.length}</div>
        </div>
        <div className="bg-black bg-opacity-30 p-2 md:p-3 rounded">
          <div className="text-cyan-300 font-bold">LATTICE STATE</div>
          <div className="text-yellow-400">{latticeExpanded ? 'EXPANDED' : 'CONTRACTED'}</div>
        </div>
        <div className="bg-black bg-opacity-30 p-2 md:p-3 rounded">
          <div className="text-cyan-300 font-bold">QUANTUM FLUX</div>
          <div className="text-blue-400">{Math.round(quantumFlux)}%</div>
        </div>
        <div className="bg-black bg-opacity-30 p-2 md:p-3 rounded">
          <div className="text-cyan-300 font-bold">USER ENGAGEMENT</div>
          <div className="text-purple-400">{userEngagement}%</div>
        </div>
      </div>

      <div className="mt-4 md:mt-6 text-center text-xs md:text-sm text-gray-300">
        Sacred Geometry • Cognitive Architecture • Quantum Consciousness Interface • Harmonic Resonance Field
      </div>
    </div>
  );
};

export default QuantumNexusSimulation;
