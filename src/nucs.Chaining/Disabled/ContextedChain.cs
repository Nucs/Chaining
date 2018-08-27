//using System;
//using System.Collections.Generic;
//using System.Threading;

//namespace Ebby {
//    public delegate (TContext ctx, CE<TContext> @delegate) CE<TContext>(Chain<TContext> chain, TContext ctx);

//    public class Chain<TContext> : BaseChain<CE<TContext>> {
//        public static Chain<TContext> Build(TContext initialContext, CE<TContext> func) { return new Chain<TContext>(initialContext, func); }

//        public TContext Context { get; protected set; }

//        public Chain(TContext initialContext, CE<TContext> script) : base(script) { Context = initialContext; }
//        protected Chain(TContext initialContext) { Context = initialContext; }

//        #region Overrides of BaseChain<CE<TContext>>

//        protected override CE<TContext> NullReturningDelegate { get; } = (chain, ctx) => (ctx, null);

//        protected override CE<TContext> InvokeDelegate(CE<TContext> @delegate) {
//            var ret = @delegate.Invoke(this, Context);
//            Context = ret.ctx;
//            return ret.@delegate;
//        }

//        #endregion
//    }
//}